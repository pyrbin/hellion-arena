using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class NetCubeSendCommandSystem : CommandSendSystem<CubeInput>
{
}

public class NetCubeReceiveCommandSystem : CommandReceiveSystem<CubeInput>
{
}

public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest>
{
}

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
            Entities.WithNone<CubeInput>().ForEach((Entity ent, ref TestComponent cube) =>
            {
                if (cube.PlayerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<CubeInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        var input = default(CubeInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        if (Input.GetKey("a"))
            input.horizontal -= 1;
        if (Input.GetKey("d"))
            input.horizontal += 1;
        if (Input.GetKey("s"))
            input.vertical -= 1;
        if (Input.GetKey("w"))
            input.vertical += 1;
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}