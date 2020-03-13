using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class MoveSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem cmdBufferSystem;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NavMap>();
        cmdBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dt = Time.fixedDeltaTime;
        var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

        inputDeps = Entities
            .ForEach((Entity e, int entityInQueryIndex, ref DynamicBuffer<Waypoint> waypoints, ref Translation translation, ref Rotation rotation) =>
            {
                if (waypoints.Length < 1)
                {
                    cmdBuffer.RemoveComponent<Waypoint>(entityInQueryIndex, e);
                    return;
                };

                var wp = waypoints[waypoints.Length - 1];
                var stepSize = 5f * dt;
                var direction = wp.Value - translation.Value;

                if (math.length(direction.xz) > 0.1f)
                {
                    direction = math.normalize(direction);
                    rotation.Value = quaternion.LookRotation(direction, math.up());
                    translation.Value.xz += direction.xz * stepSize;
                }
                else if (waypoints.Length >= 1)
                {
                    cmdBuffer.AddComponent<NavMapBuild>(entityInQueryIndex, e);
                    waypoints.RemoveAt(waypoints.Length - 1);
                }
            }).Schedule(inputDeps);

        cmdBufferSystem.AddJobHandleForProducer(inputDeps);

        inputDeps.Complete();

        return inputDeps;
    }
}