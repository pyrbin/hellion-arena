using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public class WaypointVisualizer : JobComponentSystem
{
    protected override void OnCreate() => RequireSingletonForUpdate<NetworkIdComponent>();

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();
        Entities
            .WithAll<UserCmd>()
            .ForEach((ref DynamicBuffer<Waypoint> waypoints, ref Translation translation) =>
            {
                for (int i = waypoints.Length - 1; i >= 0; i--)
                {
                    var src = i == waypoints.Length - 1 ? PositionUtil.SetY(translation.Value) : PositionUtil.SetY(waypoints[i + 1].Value);
                    Debug.DrawLine(src, PositionUtil.SetY(waypoints[i].Value), Color.magenta);
                }
            }).Run();
        return default;
    }
}
