using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct NavMapCreate : IComponentData
{
    public int3 Size;
    public float TileSize;

    public transform3d Transform;
}

public struct NavMapBuild : IComponentData
{
}

public struct NavMap : IComponentData
{
    public BlobAssetReference<NavMapBlob> Blob;
    public int Count => NodeCount(Size);
    public int3 Size => Blob.Value.Size;
    public float TileSize => Blob.Value.TileSize;
    public ref BlobArray<NavMapNode> Nodes => ref Blob.Value.Nodes;
    public unsafe NavMapNode* NodesPtr => (NavMapNode*)Blob.Value.Nodes.GetUnsafePtr();

    public transform3d Transform => Blob.Value.Transform;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToCenterPos(int3 coord, float tileSize)
        => (new float3(coord.x * tileSize, coord.y * tileSize, coord.z * tileSize) + new float3(tileSize / 2f, 0, tileSize / 2f));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(int3 coord, int3 size)
        => (coord.x * (size.y * size.z)) + (coord.y * (size.z)) + coord.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NodeCount(int3 size)
        => size.x * size.y * size.z;
}

public struct NavMapBlob
{
    public int3 Size;
    public float TileSize;
    public transform3d Transform;
    public BlobArray<NavMapNode> Nodes;
}

public struct NavMapNode
{
    public int3 Coord;
    public bool Walkable;
    public Aabb Aabb;

    public int Heuristic(int3 endPos)
    {
        int xDst = math.abs(Coord.x - endPos.x);
        int yDst = math.abs(Coord.y - endPos.y);
        int remaining = math.abs(xDst - yDst);
        return 14 * math.min(xDst, yDst) + 10 * remaining;
    }
}