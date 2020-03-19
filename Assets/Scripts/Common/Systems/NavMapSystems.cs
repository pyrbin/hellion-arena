using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.NetCode;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

public unsafe static class NavMapSystems
{
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    public class Build : JobComponentSystem
    {
        public Entity NodePrefab;

        public struct BuildRequest : IComponentData
        {
            public int3 Size;
            public float NodeSize;
            public transform3d Transform;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, ref BuildRequest req) =>
                {
                    var size = req.Size;
                    var nodeSize = req.NodeSize;
                    var transform = req.Transform;

                    var blobBuilder = new BlobBuilder(Allocator.Temp);
                    ref var navMapBlob = ref blobBuilder.ConstructRoot<NavMapBlob>();

                    var nodes = blobBuilder.Allocate(ref navMapBlob.Nodes, NavMap.NodeCount(size));

                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
                            for (int z = 0; z < size.z; z++)
                            {
                                var coord = new int3(x, y, z);
                                var centerPos = transform.GetWorldPos(NavMap.ToCenterPos(coord, nodeSize));
                                nodes[NavMap.GetIndex(coord, size)] = new NavMapNode
                                {
                                    Coord = coord,
                                    Walkable = true,
                                    Aabb = CreateBoxAABB(centerPos, new float3(1) * nodeSize, quaternion.identity)
                                };
                            }
                        }
                    }

                    EntityManager.AddComponentData(entity, new NavMap
                    {
                        Blob = blobBuilder.CreateBlobAssetReference<NavMapBlob>(Allocator.Persistent),
                        Size = size,
                        NodeSize = nodeSize,
                        Transform = transform
                    });

                    blobBuilder.Dispose();

                    EntityManager.RemoveComponent<BuildRequest>(entity);

                    EntityManager.AddComponentData(entity, new UpdateWalkables.UpdateRequest());
                }).Run();

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb CreateBoxAABB(float3 center, float3 size, quaternion orientation)
        {
            var rigidbodyBox = RigidBody.Zero;

            rigidbodyBox.Collider = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                Center = center,
                Orientation = orientation,
                Size = size,
            });

            return rigidbodyBox.CalculateAabb();
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class UpdateWalkables : JobComponentSystem
    {
        public struct UpdateRequest : IComponentData { }

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<UpdateRequest>();
            RequireSingletonForUpdate<NavMap>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var navMap = GetSingleton<NavMap>();

            var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();

            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

            inputDeps = BuildJob.Schedule(navMap.NodesPtr, ref collisionWorld, navMap.Count);

            inputDeps.Complete();

            EntityManager.RemoveComponent<UpdateRequest>(
                GetSingletonEntity<UpdateRequest>()
            );

            return inputDeps;
        }

        [BurstCompile]
        private struct BuildJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public NavMapNode* Ptr;
            [ReadOnly] public CollisionWorld World;

            public void Execute(int index)
            {
                var allHits = new NativeList<int>(Allocator.Temp);

                var input = new OverlapAabbInput
                {
                    Aabb = Ptr[index].Aabb,
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << 8, // Obstacle layer
                        GroupIndex = 0
                    }
                };

                World.OverlapAabb(input, ref allHits);
                Ptr[index].Walkable = allHits.Length <= 0;
                allHits.Dispose();
            }

            // TODO: Improve perf, ex. Specify a range of indices to check instead of whole grid
            public static JobHandle Schedule(NavMapNode* Ptr, ref CollisionWorld World, int count)
            {
                return (new BuildJob { Ptr = Ptr, World = World }).Schedule(count, 64);
            }
        }
    }
}
