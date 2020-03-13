using System;
using Unity.Entities;
using Unity.Mathematics;

public struct Map : IComponentData
{
    public int TileSize;
    public int EdgeSize;
}

[InternalBufferCapacity(25)]
public struct Tilemap : IBufferElementData
{
    public int Index;
    public int3 Coord;

    public float3 Center;
    public bool Walkable;
}