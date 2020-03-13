using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/*
/// <summary>
/// AStarSystem
/// </summary>
[AlwaysSynchronizeSystem]
public class AStarSystem : JobComponentSystem
{
    private EntityQuery MapQuery;
    private EndSimulationEntityCommandBufferSystem cmdBufferSystem;

    protected override void OnCreate()
    {
        cmdBufferSystem =
            World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        RequireForUpdate(
            MapQuery = GetEntityQuery(ComponentType.ReadOnly<Grid>(), ComponentType.ReadWrite<AStarNode>())
        );
    }

    protected override void OnCreateManager()
    {
        var gridEntity = EntityManager.CreateEntity();

        EntityManager.SetName(gridEntity, "Grid");
        EntityManager.AddComponentData(gridEntity, new Grid()
        {
        });

        var nodes = EntityManager.AddBuffer<AStarNode>(gridEntity);
        var grid = EntityManager.GetComponentData<Grid>(gridEntity);

        for (var x = 0; x < grid.GridSize.x; x++)
        {
            for (var y = 0; y < grid.GridSize.y; y++)
            {
                for (var z = 0; z < grid.GridSize.z; z++)
                {
                    var coord = new int3(x, y, z);

                    nodes.Add(new AStarNode()
                    {
                        Coord = coord,
                        Walkable = true,
                    });
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var entity = MapQuery.GetSingletonEntity();
        var tiles = EntityManager.GetBuffer<AStarNode>(entity);
        var grid = EntityManager.GetComponentData<Grid>(entity);
        var dt = Time.DeltaTime;

        Entities
                .WithReadOnly(tiles)
                .ForEach((ref PathRequest pathRequest, ref Translation translate) =>
                {
                    NativeList<int> openList = new NativeList<int>(Allocator.Temp);
                    NativeList<int> closeList = new NativeList<int>(Allocator.Temp);
                    NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
                    neighbourOffsetArray[0] = new int2(-1, 0); // Left
                    neighbourOffsetArray[1] = new int2(+1, 0); // Right
                    neighbourOffsetArray[2] = new int2(0, +1); // Up
                    neighbourOffsetArray[3] = new int2(0, -1); // Down
                    neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
                    neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
                    neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
                    neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

                    var startPos = translate.Value;
                    var endPos = pathRequest.To;

                    var startCoord = ToGridCoord(translate.Value, grid);
                    var endCoord = ToGridCoord(pathRequest.To, grid);

                    int endNodeIndex = GetIndex(endCoord, grid);

                    // Reset each node
                    for (int i = 0; i < grid.NodeCount; i++) { tiles[i].Ready(startCoord); }

                    var startNode = tiles[GetIndex(startPos, grid)];
                    startNode.G = 0;
                    tiles[startNode.Index] = startNode;

                    openList.Add(startNode.Index);

                    while (openList.Length > 0)
                    {
                        int currentNodeIndex = GetLowestFCostIndex(ref openList, ref tiles);
                        var currentNode = tiles[currentNodeIndex];

                        // Reached our destination!
                        if (currentNodeIndex == endNodeIndex)
                        {
                            break;
                        }

                        // Remove current node from Open List
                        for (int i = 0; i < openList.Length; i++)
                        {
                            if (openList[i] == currentNodeIndex)
                            {
                                openList.RemoveAtSwapBack(i);
                                break;
                            }
                        }

                        for (int i = 0; i < neighbourOffsetArray.Length; i++)
                        {
                        }

                        closeList.Add(currentNodeIndex);
                    }
                }).Run();

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLowestFCostIndex(ref NativeList<int> openList, ref DynamicBuffer<AStarNode> nodes)
    {
        var lowestCostPathNode = nodes[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            var testPathNode = nodes[openList[i]];
            if (testPathNode.F < lowestCostPathNode.F)
            {
                lowestCostPathNode = testPathNode;
            }
        }
        return lowestCostPathNode.Index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetIndex(float3 pos, [ReadOnly] Grid info)
    {
        return GetIndex(ToGridCoord(pos, info), info.GridSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetIndex(int3 coord, int3 size)
    {
        return (coord.x * (size.y * size.z)) + (coord.y * (size.z)) + coord.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int3 ToGridCoord(float3 localPos, [ReadOnly] Grid info)
    {
        return new int3(ToGridValue(localPos.x, info), ToGridValue(localPos.y, info), ToGridValue(localPos.z, info));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToGridValue(float localPos, [ReadOnly] Grid info)
    {
        return (int)math.round((localPos - (info.TileSize / 2)) / info.TileSize);
    }

    protected override void OnDestroy()
    {
        // on destroy ...
    }
}
*/