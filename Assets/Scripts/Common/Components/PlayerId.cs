using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct PlayerId : IComponentData
{
    // TODO: maybe use this value to sync movement data?

    // public float3 WalkTo
    [GhostDefaultField]
    public int Value;
}
