using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

/// <summary>
/// InputSystem
/// </summary>
[AlwaysSynchronizeSystem]
public class MouseInputSystem : JobComponentSystem
{
    private const float RAYCAST_DISTANCE = 120f;

    protected override void OnCreate() => RequireSingletonForUpdate<NavMap>();

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float3 destination;

        if (!MouseRaycast(RAYCAST_DISTANCE, out destination))
            return default;

        var indicatorColor = Input.GetMouseButton(0) ? Color.green : Color.white;
        var indicatorSize = Input.GetMouseButton(0) ? .22f : .33f;

        DebugExtension.DebugCircle(destination, indicatorColor, indicatorSize);

        if (!Input.GetMouseButtonDown(0))
        {
            return default;
        }

        var dst = destination;
        var dt = Time.fixedDeltaTime;

        Entities
            .WithAll<NavAgent>()
            .WithStructuralChanges() // Use CmdBuffer instead
            .ForEach((Entity entity) =>
            {
                EntityManager.RemoveComponent<Waypoint>(entity);
                EntityManager.AddComponentData(entity, new PathRequest { To = dst });
            })
            .WithoutBurst()
            .Run();

        return default;
    }

    // Raycast from mouse position to world
    public bool MouseRaycast(float distance, out float3 destination)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);

        RaycastInput input = new RaycastInput()
        {
            Start = ray.origin,
            End = ray.origin + ray.direction * distance,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
        };

        var hit = new RaycastHit();
        var physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        var rayCastHit = physicsWorld.CollisionWorld.CastRay(input, out hit);

        destination = hit.Position;
        return rayCastHit;
    }
}