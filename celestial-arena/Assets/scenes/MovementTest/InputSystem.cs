using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

/// <summary>
/// InputSystem
/// </summary>
[AlwaysSynchronizeSystem]
public class InputSystem : JobComponentSystem
{
    private const float RAYCAST_DISTANCE = 100f;
    private float3 destination;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var result = new RaycastHit();

        if (Input.GetMouseButton(0) && MouseRaycast(RAYCAST_DISTANCE, out result))
        {
            destination = result.Position;
        }

        var dst = destination;
        var dt = Time.fixedDeltaTime;

        Entities
            .WithName("Update_Selected_ShouldMove")
            .WithAll<Selected>()
            .ForEach((ref PhysicsVelocity vel, in Translation tr, in Speed sp) =>
            {
                if (Mathf.Abs(Vector3.Distance(tr.Value, dst)) >= 1f)
                {
                    var delta = tr.Value - dst;
                    var dir = new Vector2(delta.x, delta.z).normalized * sp.Value * dt;
                    vel.Linear.xz = -dir;
                }
            })
            .WithoutBurst()
            .Run();

        return default;
    }

    // Raycast from mouse position to world
    public bool MouseRaycast(float distance, out RaycastHit hit)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastInput input = new RaycastInput()
        {
            Start = ray.origin,
            End = ray.direction * distance,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
        };

        var physicsWorld = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld;
        return physicsWorld.CollisionWorld.CastRay(input, out hit);
    }
}