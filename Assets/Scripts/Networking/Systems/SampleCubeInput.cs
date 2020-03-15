using Unity.Entities;
using Unity.NetCode;

public class NetCubeSendCommandSystem : CommandSendSystem<MoveInput> { }

public class NetCubeReceiveCommandSystem : CommandReceiveSystem<MoveInput> { }

public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest> { }

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SampleCubeInput : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableArenaGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<MoveInput>().ForEach((Entity ent, ref Actor actor) =>
           {
               if (actor.PlayerId == localPlayerId)
               {
                   PostUpdateCommands.AddBuffer<MoveInput>(ent);
                   PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
               }
           });
        }
    }
}