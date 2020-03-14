using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct NavMapCreate : IComponentData
{
    public int3 Size;
    public float NodeSize;

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
    public float NodeSize => Blob.Value.NodeSize;
    public ref BlobArray<NavMapNode> Nodes => ref Blob.Value.Nodes;
    public unsafe NavMapNode* NodesPtr => (NavMapNode*)Blob.Value.Nodes.GetUnsafePtr();

    public transform3d Transform => Blob.Value.Transform;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotOutOfBounds(int3 coord, int3 size)
        => coord.x >= 0 &&
           coord.y >= 0 &&
           coord.z >= 0 &&
           coord.x < size.x &&
           coord.y < size.y &&
           coord.z < size.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToCenterPos(int3 coord, float nodeSize)
        => (new float3(coord.x * nodeSize, coord.y * nodeSize, coord.z * nodeSize) + new float3(nodeSize / 2f, 0, nodeSize / 2f));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(int3 coord, int3 size)
        => (coord.x * (size.y * size.z)) + (coord.y * (size.z)) + coord.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(float3 position, float nodeSize, int3 size)
    {
        return GetIndex(ToMapCoord(position, nodeSize), size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 ToMapCoord(float3 localPos, float nodeSize)
    {
        return new int3(ToMapValue(localPos.x, nodeSize), ToMapValue(localPos.y, nodeSize), ToMapValue(localPos.z, nodeSize));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToMapValue(float localPos, float nodeSize)
    {
        return (int)math.round((localPos - (nodeSize / 2)) / nodeSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NodeCount(int3 size)
        => size.x * size.y * size.z;
}

public struct NavMapBlob
{
    public int3 Size;
    public float NodeSize;
    public transform3d Transform;
    public BlobArray<NavMapNode> Nodes;
}

public struct NavMapNode
{
    public int3 Coord;
    public bool Walkable;
    public Aabb Aabb;
}