using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// A
/// </summary>
[GenerateAuthoringComponent]
public struct Actor : IComponentData
{
    [Comment("Actors are entities thats allowed to act on a turn", CommentType.Info)]
    [GhostDefaultField]
    public int PlayerId;
}