using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.NetCode;
using Unity.Mathematics;

public static class TurnLoop
{
    private struct TurnQueue : IBufferElementData
    {
        public Entity Player;
    }
}
