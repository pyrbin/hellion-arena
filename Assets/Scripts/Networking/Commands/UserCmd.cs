using System;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;
using Unity.NetCode;

[System.Serializable]
public struct UserCmd : ICommandData<UserCmd>
{
    public uint Tick => tick;

    public enum Cmd : uint
    {
        None = 0,
        Jump = 1 << 0,
    }

    public uint tick;
    public float3 MoveTo;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteFloat(MoveTo.x);
        writer.WriteFloat(MoveTo.y);
        writer.WriteFloat(MoveTo.z);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        MoveTo.x = reader.ReadFloat();
        MoveTo.y = reader.ReadFloat();
        MoveTo.z = reader.ReadFloat();
    }

    public void Serialize(ref DataStreamWriter writer, UserCmd baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, UserCmd baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }
}
