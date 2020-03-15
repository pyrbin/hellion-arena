using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

/// <summary>
/// InputSystem
/// </summary>
// TODO: think of some kind of confirm on valid action system to determine that an action has been
// done thus turn can be ended. Probly should be designed around the turn point system, eg a player
// has a certain amount of points per turn when all have been spended wait for all actions to
// complete then end turn.
[AlwaysSynchronizeSystem]
[UpdateInWorld(UpdateInWorld.TargetWorld.Client)]
[RequireComponent(typeof(Translation))]
public class TurnInputSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<TurnActor>();
        RequireSingletonForUpdate<NavMap>();
    }

    private const float RAYCAST_DISTANCE = 120f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var didAction = false;

        var entity = GetSingletonEntity<TurnActor>();
        var translation = EntityManager.GetComponentData<Translation>(entity);

        var hit = MouseRaycast(RAYCAST_DISTANCE, out var destination);

        var indicatorColor = Input.GetMouseButton(0) ? Color.green : Color.white;
        var indicatorSize = Input.GetMouseButton(0) ? .22f : .33f;

        DebugExtension.DebugCircle(destination, indicatorColor, indicatorSize);
        DebugExtension.DebugCircle(PositionUtil.ZeroY(translation.Value), Color.green, 0.66f);

        if (Input.GetMouseButtonDown(0) && hit)
        {
            // EntityManager.RemoveComponent<Waypoint>(entity);
            EntityManager.AddComponentData(entity, new PathRequest { To = destination });
        }

        if (didAction)
        {
            TurnUtil.EndTurn(EntityManager);
        }

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