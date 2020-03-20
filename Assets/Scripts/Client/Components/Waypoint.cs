using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Waypoint
/// </summary>
[InternalBufferCapacity(32)]
public struct Waypoint : IBufferElementData
{
    public float3 Value;
}
