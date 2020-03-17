using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavAgent : IComponentData
{
    public float Speed;
}

public struct PathRequest : IComponentData
{
    public float3 To;
}
