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
using System.Linq;

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

    public uint tick;
    public BitField Actions;

    // MoveOrder variables
    public float3 moveOrderTo;

    public static readonly UserCmd empty = new UserCmd(0);

    private UserCmd(int _)
    {
        tick = 0;
        Actions = new BitField { Flags = 0 };
        moveOrderTo = float3.zero;
    }

    public void ClearCommand(uint tick = 0)
    {
        Actions.Flags = 0;
        moveOrderTo = float3.zero;
        this.tick = tick;
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUInt(Actions.Flags);

        writer.WriteFloat(moveOrderTo.x);
        writer.WriteFloat(moveOrderTo.y);
        writer.WriteFloat(moveOrderTo.z);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        Actions.Flags = reader.ReadUInt();

        moveOrderTo.x = reader.ReadFloat();
        moveOrderTo.y = reader.ReadFloat();
        moveOrderTo.z = reader.ReadFloat();
    }

    public void Serialize(ref DataStreamWriter writer, UserCmd baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, UserCmd baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
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

        public void Set(Action button, bool val = true)
        {
            if (val)
                Flags |= (uint)button;
            else
            {
                Flags &= ~(uint)button;
            }
        }
    }
}

// User cmd send
public class UserCmdSendCommandSystem : CommandSendSystem<UserCmd>
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return base.OnUpdate(inputDeps);
    }
}

// User cmd receive
public class UserCmdReceiveCommandSystem : CommandReceiveSystem<UserCmd>
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return base.OnUpdate(inputDeps);
    }
}
