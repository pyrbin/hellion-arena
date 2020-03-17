using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;

/// <summary>
/// Waypoint
/// </summary>
[InternalBufferCapacity(32)]
public struct MoveToCommand : ICommandData<MoveToCommand>
{
    public uint Tick => tick;
    public uint tick;

    public float3 To;
    public bool Move;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteFloat(To.x);
        writer.WriteFloat(To.y);
        writer.WriteFloat(To.z);
        writer.WriteInt(Convert.ToInt32(Move));
    }

    public void Serialize(ref DataStreamWriter writer, MoveToCommand baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        To.x = reader.ReadFloat();
        To.y = reader.ReadFloat();
        To.z = reader.ReadFloat();
        Move = Convert.ToBoolean(reader.ReadInt());
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, MoveToCommand baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }
}

public class MoveToCommandSendCommandSystem : CommandSendSystem<MoveToCommand>
{
}

public class MoveToCommandReceiveCommandSystem : CommandReceiveSystem<MoveToCommand>
{
}
