using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

/// <summary>
/// InputSystem
/// </summary>
[AlwaysSynchronizeSystem]
public class MouseInputSystem : JobComponentSystem
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
        else
        {
            return default;
        }

        var dst = (destination);
        var dt = Time.fixedDeltaTime;

        Entities
            .WithAll<Player>()
            .WithStructuralChanges() // Use CmdBuffer instead
            .ForEach((Entity entt) =>
            {
                if (EntityManager.HasComponent<Waypoint>(entt))
                {
                    EntityManager.RemoveComponent<Waypoint>(entt);
                }
                // EntityManager.AddComponentData(entt, new PathRequest { To = dst });
                var waypoints = EntityManager.AddBuffer<Waypoint>(entt);
                waypoints.Add(new Waypoint() { Value = dst });
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