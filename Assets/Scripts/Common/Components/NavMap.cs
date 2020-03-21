using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.NetCode;

public unsafe struct NavMap : IComponentData
{
    public int3 Size;

    public float NodeSize;

    public transform3d Transform;

    public BlobAssetReference<NavMapBlob> Blob;
    public ref BlobArray<NavMapNode> Nodes => ref Blob.Value.Nodes;
    public NavMapNode* NodesPtr => (NavMapNode*)Blob.Value.Nodes.GetUnsafePtr();

    public int Count => NodeCount(Size);

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OutOfBounds(int3 coord, int3 size)
        => !(coord.x >= 0 &&
           coord.y >= 0 &&
           coord.z >= 0 &&
           coord.x < size.x &&
           coord.y < size.y &&
           coord.z < size.z);

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

    #endregion Helpers
}

public struct NavMapBlob
{
    public BlobArray<NavMapNode> Nodes;
}

public struct NavMapNode
{
    public int3 Coord;
    public float3 Center;
    public bool Walkable;
}
