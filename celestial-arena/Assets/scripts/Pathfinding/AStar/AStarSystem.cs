using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// AStarSystem
/// </summary>
public unsafe class AStarSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem cmdBufferSystem;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NavMap>();
        cmdBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct AStarCost
    {
        public int G;
        public int H;
        public int F => H + G;

        public void Set(int G, int H)
        {
            this.G = G;
            this.H = H;
        }

        public void Reset()
        {
            G = 0;
            H = 0;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

        var navMap = GetSingleton<NavMap>();
        var count = navMap.Count;
        var nodeSize = navMap.NodeSize;
        var mapSize = navMap.Size;
        var ptr = navMap.NodesPtr;
        var transform = navMap.Transform;

        inputDeps = Entities
            .WithNone<Waypoint>()
            .WithNativeDisableUnsafePtrRestriction(ptr)
            .ForEach((Entity entity, int entityInQueryIndex, ref PathRequest req, ref Translation translation) =>
            {
                NativeArray<AStarCost> Costs = new NativeArray<AStarCost>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                NativeArray<bool> CloseSet = new NativeArray<bool>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                NativeArray<int> CameFrom = new NativeArray<int>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                NativeMinHeap minSet = new NativeMinHeap(count * 4, Allocator.Temp);

                NativeArray<int2> neighbours = new NativeArray<int2>(8, Allocator.Temp);

                GetNeighbours(ref neighbours);

                for (int i = 0; i < count; i++)
                {
                    Costs[i] = new AStarCost { H = 0, G = int.MaxValue };
                    CloseSet[i] = false;
                    CameFrom[i] = -1;
                }

                int startNode = NavMap.GetIndex(translation.Value, nodeSize, mapSize);
                int endNode = NavMap.GetIndex(req.To, nodeSize, mapSize);

                Costs[startNode].Reset();

                int lastNode = startNode;
                while (lastNode != endNode && lastNode != -1)
                {
                    // Mark current tile as visited
                    CloseSet[lastNode] = true;
                    NavMapNode node = ptr[lastNode];

                    for (int i = 0; i < neighbours.Length; ++i)
                    {
                        int2 neighbourOffset = neighbours[i];
                        int3 neighbourCoord = new int3(node.Coord.x + neighbours[i].x, node.Coord.y, node.Coord.z + neighbours[i].y);

                        if (!NavMap.NotOutOfBounds(neighbourCoord, mapSize))
                        {
                            continue;
                        }

                        int neighbourIdx = NavMap.GetIndex(neighbourCoord, mapSize);
                        NavMapNode neighbourNode = ptr[neighbourIdx];

                        if (CloseSet[neighbourIdx] || !neighbourNode.Walkable)
                        {
                            continue;
                        }

                        int tentativeGCost = Costs[lastNode].G + Heuristic(node.Coord, neighbourCoord);
                        if (tentativeGCost < Costs[neighbourIdx].G)
                        {
                            Costs[neighbourIdx].Set(tentativeGCost, Costs[neighbourIdx].H);
                            CameFrom[neighbourIdx] = lastNode;
                            if (!CloseSet[neighbourIdx])
                            {
                                minSet.Push(new MinHeapNode(neighbourIdx, Costs[neighbourIdx].F));
                            }
                        }
                    }

                    lastNode = FindNext(minSet, ref CloseSet);
                }

                var waypoints = cmdBuffer.AddBuffer<Waypoint>(entityInQueryIndex, entity);

                // Travel back through path
                while (lastNode != -1)
                {
                    var coord = ptr[lastNode].Coord;
                    var worldPos = transform.GetWorldPos(NavMap.ToCenterPos(coord, nodeSize));

                    waypoints.Add(new Waypoint() { Value = new float3(worldPos.x, translation.Value.y, worldPos.z) });
                    lastNode = CameFrom[lastNode];
                }

                cmdBuffer.RemoveComponent<PathRequest>(entityInQueryIndex, entity);

                Costs.Dispose();
                CloseSet.Dispose();
                CameFrom.Dispose();
                minSet.Dispose();
                neighbours.Dispose();
            }).Schedule(inputDeps);

        cmdBufferSystem.AddJobHandleForProducer(inputDeps);

        inputDeps.Complete();

        return inputDeps;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindNext(NativeMinHeap minSet, ref NativeArray<bool> closeSet)
    {
        while (minSet.HasNext())
        {
            var next = minSet.Pop();
            // Check if this is not visited tile
            if (!closeSet[next.Position])
            {
                return next.Position;
            }
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetNeighbours(ref NativeArray<int2> neighbourOffsetArray)
    {
        neighbourOffsetArray[0] = new int2(-1, 0); // Left
        neighbourOffsetArray[1] = new int2(+1, 0); // Right
        neighbourOffsetArray[2] = new int2(0, +1); // Up
        neighbourOffsetArray[3] = new int2(0, -1); // Down
        neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
        neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
        neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
        neighbourOffsetArray[7] = new int2(+1, +1); // Right Up
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Heuristic(int3 coord, int3 endPos)
    {
        int xDst = math.abs(coord.x - endPos.x);
        int yDst = math.abs(coord.y - endPos.y);
        int remaining = math.abs(xDst - yDst);
        return 14 * math.min(xDst, yDst) + 10 * remaining;
    }

    /*
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
    */
}