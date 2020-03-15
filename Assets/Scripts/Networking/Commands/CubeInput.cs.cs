using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;

public struct MoveInput : ICommandData<MoveInput>
{
    public uint Tick => tick;
    public uint tick;

    public float x;
    public float y;
    public float z;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteFloat(x);
        writer.WriteFloat(y);
        writer.WriteFloat(z);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        x = reader.ReadFloat();
        y = reader.ReadFloat();
        z = reader.ReadFloat();
    }

    public void Serialize(ref DataStreamWriter writer, MoveInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, MoveInput baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }
}