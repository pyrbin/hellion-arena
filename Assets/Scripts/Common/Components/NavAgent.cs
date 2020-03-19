using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavAgent : IComponentData
{
    // TODO: maybe use this value to sync movement data?

    // public float3 WalkTo
    public float Speed;
}
