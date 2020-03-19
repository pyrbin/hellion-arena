using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Waypoint
/// </summary>
[GenerateAuthoringComponent]
[InternalBufferCapacity(32)]
public struct Waypoint : IBufferElementData
{
    public float3 Value;
}
