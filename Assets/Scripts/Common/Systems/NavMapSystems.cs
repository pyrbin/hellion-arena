using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.NetCode;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using Unity.Transforms;

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
                                    Center = centerPos,
                                    Walkable = true,
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
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class UpdateWalkables : JobComponentSystem
    {
        public struct UpdateRequest : IComponentData { }

        private const float RAYCAST_DISTANCE = 5f;
        private const float NODE_SIZE_MULT = 0.75f;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<UpdateRequest>();
            RequireSingletonForUpdate<NavMap>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

            var navMap = GetSingleton<NavMap>();
            var ptr = navMap.NodesPtr;

            inputDeps = (new ResetNavMapJob { Ptr = ptr }).Schedule(navMap.Count, 32, inputDeps);

            inputDeps.Complete();

            inputDeps = Entities
                .WithNativeDisableUnsafePtrRestriction(ptr)
                .WithAll<NavObstacle>()
                .ForEach((ref PhysicsCollider collider, ref Translation translation, ref Rotation rotation) =>
                {
                    var ribt = new RigidTransform(rotation.Value, translation.Value);
                    var aabb = collider.ColliderPtr->CalculateAabb(ribt);
                    var inc = navMap.NodeSize * .5f;
                    for (var x = aabb.Min.x; x < aabb.Max.x; x += inc)
                    {
                        for (var z = aabb.Min.z; z < aabb.Max.z; z += inc)
                        {
                            var localPos = navMap.Transform.GetLocalPos(new float3(x, 0, z));
                            var coords = NavMap.ToMapCoord(localPos, navMap.NodeSize);

                            if (NavMap.OutOfBounds(coords, navMap.Size)) continue;

                            var idx = NavMap.GetIndex(coords, navMap.Size);
                            var from = ptr[idx].Center;
                            var to = from + new float3(0, RAYCAST_DISTANCE, 0);
                            ptr[idx].Walkable = !SphereCast(from, to, (navMap.NodeSize * NODE_SIZE_MULT) / 2f, ref collisionWorld, 8);
                        }
                    }
                }).Schedule(inputDeps);

            inputDeps.Complete();

            EntityManager.RemoveComponent<UpdateRequest>(
                GetSingletonEntity<UpdateRequest>()
            );

            return inputDeps;
        }

        [BurstCompile]
        private struct ResetNavMapJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            public NavMapNode* Ptr;

            public void Execute(int index)
            {
                Ptr[index].Walkable = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SphereCast(float3 from, float3 to, float radius, ref CollisionWorld world, int layer = 0)
        {
            var filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << layer,
                GroupIndex = 0
            };
            // TODO: cache colliders?
            var collider = SphereCollider.Create(new SphereGeometry
            {
                Center = float3.zero,
                Radius = radius,
            }, filter);

            var hit = world.CastCollider(new ColliderCastInput()
            {
                Collider = (Collider*)collider.GetUnsafePtr(),
                Orientation = quaternion.identity,
                Start = from,
                End = to
            });

            collider.Dispose();

            return hit;
        }
    }
}
