using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavAgent : IComponentData
{
}

public struct PathRequest : IComponentData
{
    public float3 To;
}