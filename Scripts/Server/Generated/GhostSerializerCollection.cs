using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct HellionGhostSerializerCollection : IGhostSerializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
        };
        return arr;
    }

    public int Length => 0;
#endif
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        return -1;
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(ref DataStreamWriter dataStream, SerializeData data)
    {
        switch (data.ghostType)
        {
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
}

public struct EnableHellionGhostSendSystemComponent : IComponentData
{}
public class HellionGhostSendSystem : GhostSendSystem<HellionGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableHellionGhostSendSystemComponent>();
    }
}
