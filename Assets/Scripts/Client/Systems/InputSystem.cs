using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// InputSystem
/// </summary>
public static class InputSystem
{
    private static UserCmd Cmd;
    private static Entity Player;
    private static uint Tick;

    [UpdateInGroup(typeof(ClientSimulationSystemGroup))]
    [UpdateBefore(typeof(GhostSimulationSystemGroup))]
    public class InputSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            Cmd = UserCmd.empty;
            RequireSingletonForUpdate<NetworkIdComponent>();
            RequireSingletonForUpdate<EnableHellionGhostReceiveSystemComponent>();
        }

        protected override void OnUpdate()
        {
            if (GetPlayerEntity(out Player))
            {
                Cmd.ClearCommand(Tick);
                Tick = ServerTick(World);
                base.OnUpdate();
            }
        }

        private bool GetPlayerEntity(out Entity entity)
        {
            entity = GetSingleton<CommandTargetComponent>().targetEntity;
            if (entity == Entity.Null)
            {
                var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
                Entities.WithNone<UserCmd>().ForEach((Entity ent, ref PlayerId player) =>
                {
                    if (player.Value == localPlayerId)
                    {
                        PostUpdateCommands.AddBuffer<UserCmd>(ent);
                        PostUpdateCommands.AddBuffer<Waypoint>(ent);
                        PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                    }
                });
                return false;
            }
            else
            {
                return true;
            }
        }

        private static uint ServerTick(World world)
            => (world.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick);
    }

    [UpdateInGroup(typeof(InputSystemGroup))]
    public unsafe class MoveToWaypoints : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var waypoints = EntityManager.GetBuffer<Waypoint>(Player);

            if (waypoints.Length < 1) return;

            var position = EntityManager.GetComponentData<Translation>(Player).Value;
            var toPosition = waypoints[waypoints.Length - 1].Value;

            if (math.length((toPosition - position).xz) > 0.1f)
            {
                Cmd.moveOrderTo = toPosition;
                Cmd.Actions.Set(UserCmd.Action.MoveOrder);
            }
            else if (waypoints.Length >= 1)
            {
                waypoints.RemoveAt(waypoints.Length - 1);
                EntityManager.AddComponent<NavMapSystems.UpdateWalkables.UpdateRequest>(GetSingletonEntity<NavMap>());
            }
        }
    }

    [UpdateInGroup(typeof(InputSystemGroup))]
    [UpdateAfter(typeof(MoveToWaypoints))]
    public class TurnInputs : ComponentSystem
    {
        private const float RAYCAST_DISTANCE = 120f;

        protected unsafe override void OnUpdate()
        {
            var hit = MouseRaycast(World, RAYCAST_DISTANCE, out var destination);
            var position = EntityManager.GetComponentData<Translation>(Player).Value;

#if UNITY_EDITOR
            var indicatorColor = Input.GetMouseButton(0) ? Color.green : Color.white;
            var indicatorSize = Input.GetMouseButton(0) ? .22f : .33f;

            DebugDraw.Sphere(destination, indicatorSize, indicatorColor);
            DebugDraw.Sphere(PositionUtil.SetY(position), 0.66f, Color.green);
#endif
            if (hit)
            {
                // Start Process Input
                if (Input.GetMouseButtonDown(0) && !Cmd.Actions.Has(UserCmd.Action.MoveOrder))
                {
                    EntityManager.AddComponentData(Player, new MoveOrder.RequestComponent
                    {
                        To = PositionUtil.SetY(destination, position.y),
                        From = position
                    });
                }
            }

            // End Process Input
            EntityManager
                .GetBuffer<UserCmd>(Player)
                .AddCommandData(Cmd);
        }

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
                Filter = CollisionFilter.Default
            };

            var physicsWorld = world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

            var rayCastHit = physicsWorld.CollisionWorld.CastRay(input, out var hit);

            destination = hit.Position;
            return rayCastHit;
        }
    }
}
