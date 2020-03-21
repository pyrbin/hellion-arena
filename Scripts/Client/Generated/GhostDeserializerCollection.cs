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
            "UnitGhostSerializer",
        };
        return arr;
    }

    public int Length => 1;
#endif
    public void Initialize(World world)
    {
        var curUnitGhostSpawnSystem = world.GetOrCreateSystem<UnitGhostSpawnSystem>();
        m_UnitSnapshotDataNewGhostIds = curUnitGhostSpawnSystem.NewGhostIds;
        m_UnitSnapshotDataNewGhosts = curUnitGhostSpawnSystem.NewGhosts;
        curUnitGhostSpawnSystem.GhostType = 0;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_UnitSnapshotDataFromEntity = system.GetBufferFromEntity<UnitSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<HellionGhostDeserializerCollection>.InvokeDeserialize(m_UnitSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
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
                m_UnitSnapshotDataNewGhostIds.Add(ghostId);
                m_UnitSnapshotDataNewGhosts.Add(GhostReceiveSystem<HellionGhostDeserializerCollection>.InvokeSpawn<UnitSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<UnitSnapshotData> m_UnitSnapshotDataFromEntity;
    private NativeList<int> m_UnitSnapshotDataNewGhostIds;
    private NativeList<UnitSnapshotData> m_UnitSnapshotDataNewGhosts;
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
