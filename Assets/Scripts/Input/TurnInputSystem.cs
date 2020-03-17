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
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class TurnInputSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableHellionGhostReceiveSystemComponent>();
        RequireSingletonForUpdate<NavMap>();
    }

    private const float RAYCAST_DISTANCE = 120f;

    protected override void OnUpdate()
    {
        var entity = GetSingleton<CommandTargetComponent>().targetEntity;

        // Setup
        if (entity == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<MoveToCommand>().ForEach((Entity entt, ref Actor actor) =>
            {
                if (actor.PlayerId != localPlayerId) return;
                PostUpdateCommands.AddBuffer<MoveToCommand>(entt);
                PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = entt });
            });
            return;
        }

        var translation = EntityManager.GetComponentData<Translation>(entity);
        var hit = MouseRaycast(RAYCAST_DISTANCE, out var destination);

#if UNITY_EDITOR
        var indicatorColor = Input.GetMouseButton(0) ? Color.green : Color.white;
        var indicatorSize = Input.GetMouseButton(0) ? .22f : .33f;
        DebugExtension.DebugCircle(destination, indicatorColor, indicatorSize);
        DebugExtension.DebugCircle(PositionUtil.SetY(translation.Value), Color.green, 0.66f);
#endif
        var moveTo = Input.GetMouseButtonDown(0) && hit;
        Log.Print("{0}", moveTo);
        var cmd = new MoveToCommand { To = PositionUtil.SetY(destination, translation.Value.y), Move = moveTo };
        cmd.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        var cmdBuffer = EntityManager.GetBuffer<MoveToCommand>(entity);
        cmdBuffer.AddCommandData(cmd);
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
