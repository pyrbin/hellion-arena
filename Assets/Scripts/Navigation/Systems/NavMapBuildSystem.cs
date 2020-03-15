using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public unsafe class NavMapBuildSystem : JobComponentSystem
{
    public NativeMultiHashMap<int, bool> Walkables;

    protected override void OnCreate() => RequireSingletonForUpdate<NavMapBuild>();

    [BurstCompile]
    private struct NavMapBuildJob : IJobParallelFor
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
            return (new NavMapBuildJob { Ptr = Ptr, World = World }).Schedule(count, 8);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var navMap = GetSingleton<NavMap>();
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

        inputDeps = NavMapBuildJob.Schedule(navMap.NodesPtr, ref collisionWorld, navMap.Count);
        inputDeps.Complete();
        EntityManager.RemoveComponent<NavMapBuild>(GetSingletonEntity<NavMapBuild>());
        return inputDeps;
    }

    protected override void OnDestroy()
    {
        if (Walkables.IsCreated) Walkables.Dispose();
    }
}