using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

public static class NavigationMapSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [AlwaysSynchronizeSystem]
    public unsafe class CreateNodes : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, ref CreateNavigationMap create) =>
                {
                    var size = create.Size;
                    var nodeSize = create.NodeSize;
                    var transform = create.Transform;

                    var blobBuilder = new BlobBuilder(Allocator.Temp);
                    ref var navMapBlob = ref blobBuilder.ConstructRoot<NavigationMapBlob>();

                    var nodes = blobBuilder.Allocate(ref navMapBlob.Nodes, NavigationMap.NodeCount(size));

                    navMapBlob.Size = size;
                    navMapBlob.NodeSize = nodeSize;
                    navMapBlob.Transform = transform;

                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
                            for (int z = 0; z < size.z; z++)
                            {
                                var coord = new int3(x, y, z);
                                var centerPos = transform.GetWorldPos(NavigationMap.ToCenterPos(coord, nodeSize));

                                nodes[NavigationMap.GetIndex(coord, size)] = new NavigationMapNode
                                {
                                    Coord = coord,
                                    Walkable = true,
                                    Aabb = CreateBoxAABB(centerPos, new float3(1) * nodeSize, quaternion.identity)
                                };
                            }
                        }
                    }

                    // Add NavMap component
                    EntityManager.AddComponentData(entity, new NavigationMap
                    {
                        Blob = blobBuilder.CreateBlobAssetReference<NavigationMapBlob>(Allocator.Persistent)
                    });

                    blobBuilder.Dispose();

                    // Remove NavMap create component
                    EntityManager.RemoveComponent<CreateNavigationMap>(entity);

                    // Push NavMapBuild (build obstacles)
                    EntityManager.AddComponentData(entity, new BuildNavigationMap());
                }).Run();

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb CreateBoxAABB(float3 center, float3 size, quaternion orientation)
        {
            var geometry = new BoxGeometry
            {
                Center = center,
                Orientation = orientation,
                Size = size,
            };

            var rigidbodyBox = RigidBody.Zero;
            rigidbodyBox.Collider = BoxCollider.Create(geometry);
            return rigidbodyBox.CalculateAabb();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public unsafe class BuildMap : JobComponentSystem
    {
        public NativeMultiHashMap<int, bool> Walkables;

        protected override void OnCreate() => RequireSingletonForUpdate<BuildNavigationMap>();

        [BurstCompile]
        private struct BuildJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public NavigationMapNode* Ptr;
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
            public static JobHandle Schedule(NavigationMapNode* Ptr, ref CollisionWorld World, int count)
            {
                return (new BuildJob { Ptr = Ptr, World = World }).Schedule(count, 8);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var navMap = GetSingleton<NavigationMap>();
            var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

            inputDeps = BuildJob.Schedule(navMap.NodesPtr, ref collisionWorld, navMap.Count);
            inputDeps.Complete();
            EntityManager.RemoveComponent<BuildNavigationMap>(GetSingletonEntity<BuildNavigationMap>());
            return inputDeps;
        }

        protected override void OnDestroy()
        {
            if (Walkables.IsCreated) Walkables.Dispose();
        }
    }
}
