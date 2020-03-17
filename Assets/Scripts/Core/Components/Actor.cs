using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// A
/// </summary>
[GenerateAuthoringComponent]
public struct Actor : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;
}