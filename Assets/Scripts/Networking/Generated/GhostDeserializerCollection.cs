using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct HellionGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "ActorGhostSerializer",
        };
        return arr;
    }

    public int Length => 1;
#endif
    public void Initialize(World world)
    {
        var curActorGhostSpawnSystem = world.GetOrCreateSystem<ActorGhostSpawnSystem>();
        m_ActorSnapshotDataNewGhostIds = curActorGhostSpawnSystem.NewGhostIds;
        m_ActorSnapshotDataNewGhosts = curActorGhostSpawnSystem.NewGhosts;
        curActorGhostSpawnSystem.GhostType = 0;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_ActorSnapshotDataFromEntity = system.GetBufferFromEntity<ActorSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<HellionGhostDeserializerCollection>.InvokeDeserialize(m_ActorSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_ActorSnapshotDataNewGhostIds.Add(ghostId);
                m_ActorSnapshotDataNewGhosts.Add(GhostReceiveSystem<HellionGhostDeserializerCollection>.InvokeSpawn<ActorSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<ActorSnapshotData> m_ActorSnapshotDataFromEntity;
    private NativeList<int> m_ActorSnapshotDataNewGhostIds;
    private NativeList<ActorSnapshotData> m_ActorSnapshotDataNewGhosts;
}
public struct EnableHellionGhostReceiveSystemComponent : IComponentData
{}
public class HellionGhostReceiveSystem : GhostReceiveSystem<HellionGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableHellionGhostReceiveSystemComponent>();
    }
}
