using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavigationAgent : IComponentData
{
    public float Speed;
}

public struct PathRequest : IComponentData
{
    public float3 To;
}
