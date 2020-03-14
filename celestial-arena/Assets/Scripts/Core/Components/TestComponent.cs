using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct TestComponent : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;
}