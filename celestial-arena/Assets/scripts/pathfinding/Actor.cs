using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Player : IComponentData
{
}

/// <summary>
/// Actor
/// </summary>
public struct PathRequest : IComponentData
{
    public float3 To;
}