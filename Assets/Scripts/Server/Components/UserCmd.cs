using System;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;
using Unity.NetCode;
using Unity.Jobs;
using Unity.Collections;

[Serializable]
[GhostDefaultComponent(GhostDefaultComponentAttribute.Type.PredictedClient)]
public struct UserCmd : ICommandData<UserCmd>
{
    public uint Tick => tick;

    public enum Action : uint
    {
        None = 0,
        MoveOrder = 1 << 1,
        OtherAction = 1 << 2,
    }

    public struct BitField

    {
        public uint Flags { get; set; }

        public bool Has(Action flag)
        {
            return (Flags & (uint)flag) > 0;
        }

        public void Or(Action button, bool val)
        {
            if (val)
                Flags |= (uint)button;
        }

        public void Set(Action button, bool val)
        {
            if (val)
                Flags |= (uint)button;
            else
            {
                Flags &= ~(uint)button;
            }
        }
    }

    public uint tick;
    public BitField Actions;
    public float3 moveOrderPos;

    public static readonly UserCmd empty = new UserCmd(0);

    // Structs cant have parameterless constructor?
    private UserCmd(int i)
    {
        tick = 0;
        Actions = new BitField { Flags = 0 };
        moveOrderPos = float3.zero;
    }

    public void ClearCommand(uint tick = 0)
    {
        Actions.Flags = 0;
        moveOrderPos = float3.zero;
        this.tick = tick;
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUInt(Actions.Flags);
        writer.WriteFloat(moveOrderPos.x);
        writer.WriteFloat(moveOrderPos.y);
        writer.WriteFloat(moveOrderPos.z);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        Actions.Flags = reader.ReadUInt();
        moveOrderPos.x = reader.ReadFloat();
        moveOrderPos.y = reader.ReadFloat();
        moveOrderPos.z = reader.ReadFloat();
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

// User cmd send
public class UserSendCommandSystem : CommandSendSystem<UserCmd>
{
}

// User cmd receive
public class UserCmdReceiveCommandSystem : CommandReceiveSystem<UserCmd>
{
}
