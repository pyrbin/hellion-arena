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

public static class PlayerInput
{
    private static UserCmd Cmd;

    [UpdateInGroup(typeof(ClientSimulationSystemGroup))]
    [UpdateAfter(typeof(GhostSimulationSystemGroup))]
    public class HandleTurnInputs : ComponentSystem
    {
        protected override void OnCreate()
        {
            Cmd = UserCmd.empty;
            // Network is initialized
            RequireSingletonForUpdate<NetworkIdComponent>();
            //RequireSingletonForUpdate<EnableHellionGhostReceiveSystemComponent>();

            // NavigationMap is initialized/created
            RequireSingletonForUpdate<NavMap>();
        }

        private const float RAYCAST_DISTANCE = 120f;

        protected override void OnUpdate()
        {
            var entity = GetSingleton<CommandTargetComponent>().targetEntity;
            /*
            if (entity == Entity.Null)
            {
                var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
                Entities.WithNone<UserCmd>().ForEach((Entity ent, ref Actor actor) =>
                {
                    if (actor.PlayerId == localPlayerId)
                    {
                        PostUpdateCommands.AddBuffer<UserCmd>(ent);
                        PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                    }
                });
                return;
            }
            */
            return;
            var translation = EntityManager.GetComponentData<Translation>(entity);
            var hit = MouseRaycast(World, RAYCAST_DISTANCE, out var destination);

#if UNITY_EDITOR
            var indicatorColor = Input.GetMouseButton(0) ? Color.green : Color.white;
            var indicatorSize = Input.GetMouseButton(0) ? .22f : .33f;
            DebugExtension.DebugCircle(destination, indicatorColor, indicatorSize);
            DebugExtension.DebugCircle(PositionUtil.SetY(translation.Value), Color.green, 0.66f);
#endif
            Cmd.ClearCommand(ServerTick(World));

            // Start Process Input
            if (Input.GetMouseButtonDown(0) && hit && !EntityManager.HasComponent<Waypoint>(entity))
            {
                Cmd.moveOrderPos = PositionUtil.SetY(destination, translation.Value.y);
                Cmd.Actions.Set(UserCmd.Action.MoveOrder, true);
            }

            // Start Process Input
            if (Input.GetMouseButtonDown(1) && EntityManager.HasComponent<Waypoint>(entity))
            {
                EntityManager.RemoveComponent<Waypoint>(entity);
            }

            // End Process Input
            EntityManager
                .GetBuffer<UserCmd>(entity)
                .AddCommandData(Cmd);
        }

        // Get server tick
        public static uint ServerTick(World world) => (world.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick);

        // Raycast from mouse position to world
        public static bool MouseRaycast(World world, float distance, out float3 destination)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Draw ray
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

            var physicsWorld = world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

            var rayCastHit = physicsWorld.CollisionWorld.CastRay(input, out var hit);

            destination = hit.Position;
            return rayCastHit;
        }
    }
}
