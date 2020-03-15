using Unity.Entities;

[InternalBufferCapacity(8)]
public struct TurnQueue : IBufferElementData
{
    public Entity Entity;
}