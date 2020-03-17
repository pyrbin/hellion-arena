using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class WaypointSystem : JobComponentSystem
{
    private readonly static bool REQUEST_BUILD_MAP_ON_MOVE = false;
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
            .ForEach((Entity e, int entityInQueryIndex, ref DynamicBuffer<Waypoint> waypoints, ref Translation translation, ref Rotation rotation, ref NavAgent agent) =>
            {
                if (waypoints.Length < 1)
                {
                    cmdBuffer.RemoveComponent<Waypoint>(entityInQueryIndex, e);
                    // TODO: Maybe not request build on every move
                    if (REQUEST_BUILD_MAP_ON_MOVE)
                        cmdBuffer.AddComponent<NavMapBuild>(entityInQueryIndex, e);
                    return;
                }

#if UNITY_EDITOR
                // Draw lines
                for (int i = waypoints.Length - 1; i >= 0; i--)
                {
                    var src = i == waypoints.Length - 1 ? translation.Value : waypoints[i + 1].Value;
                    Debug.DrawLine(src, waypoints[i].Value, Color.green);
                }
#endif
                var wp = waypoints[waypoints.Length - 1];
                var stepSize = agent.Speed * dt;
                var direction = wp.Value - translation.Value;

                if (math.length(direction.xz) > 0.1f)
                {
                    direction = math.normalize(direction);
                    rotation.Value = quaternion.LookRotation(direction, math.up());
                    translation.Value.xz += direction.xz * stepSize;
                }
                else if (waypoints.Length >= 1)
                {
                    waypoints.RemoveAt(waypoints.Length - 1);
                }
            }).Schedule(inputDeps);

        cmdBufferSystem.AddJobHandleForProducer(inputDeps);

        inputDeps.Complete();

        return inputDeps;
    }
}
