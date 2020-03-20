using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using UnityEngine;
using static ConfigFile;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class GameClientServerControl : ComponentSystem
{
    // Singleton component to trigger connections once from a control system
    private struct InitGameComponent : IComponentData { }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitGameComponent>();
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());

        foreach (var world in World.All)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
#if !UNITY_SERVER
            // Client connection
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                var config = ServerConfig.Default;

                if (!ConfigFile.Exists())
                {
                    ConfigFile.Save(config);
                }
                else
                {
                    ConfigFile.Load(out config);
                }

                Debug.Log(config.Ip + " " + config.Port);

                // Client worlds automatically connect
                network.Connect(config.EndPoint);
            }
#endif

#if UNITY_SERVER || UNITY_EDITOR
            // Server connection
            if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                // Server world automatically listens for connections from any host
                network.Listen(ServerConfig.Default.EndPoint);
            }
#endif
        }
    }
}

[BurstCompile]
public struct GoInGameRequest : IRpcCommand
{
    // Unused integer for demonstration
    public int value;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(value);
    }

    public void Deserialize(ref DataStreamReader reader)
    {
        value = reader.ReadInt();
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<GoInGameRequest>(ref parameters);
    }

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
    }
}

// The system that makes the RPC request component transfer
public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest> { }

// When client has a connection with network id, go in game and tell server to also go in game
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class GoInGameClientSystem : ComponentSystem
{
    protected override void OnCreate() => RequireSingletonForUpdate<EnableHellionGhostReceiveSystemComponent>();

    protected override void OnUpdate() => Entities
        .WithAll<NetworkIdComponent>()
        .WithNone<NetworkStreamInGame>()
        .ForEach((Entity ent) =>
        {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(ent);
            var req = PostUpdateCommands.CreateEntity();
            PostUpdateCommands.AddComponent<GoInGameRequest>(req);
            PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
        });
}

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem
{
    protected override void OnCreate() => RequireSingletonForUpdate<EnableHellionGhostSendSystemComponent>();

    protected override void OnUpdate() => Entities
        .WithNone<SendRpcCommandRequestComponent>()
        .ForEach((Entity entity, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent rpcCmd) =>
        {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(rpcCmd.SourceConnection);
            UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game", EntityManager.GetComponentData<NetworkIdComponent>(rpcCmd.SourceConnection).Value));
            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();

            var ghostId = HellionGhostSerializerCollection.FindGhostType<UnitSnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            var player = EntityManager.Instantiate(prefab);

            EntityManager.SetComponentData(player, new PlayerId { Value = EntityManager.GetComponentData<NetworkIdComponent>(rpcCmd.SourceConnection).Value });

            PostUpdateCommands.AddBuffer<UserCmd>(player);
            PostUpdateCommands.SetComponent(rpcCmd.SourceConnection, new CommandTargetComponent { targetEntity = player });
            PostUpdateCommands.DestroyEntity(entity);
        });
}
