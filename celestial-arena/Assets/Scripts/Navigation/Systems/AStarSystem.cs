using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

// AStarSystem
// TODO: Add more advanced Pathfinding, avoidance, agent size/height, 3D navigation
public unsafe class AStarSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem cmdBufferSystem;
    private NativeArray<Neighbour> neighbours;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NavMap>();
        cmdBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        neighbours = new NativeArray<Neighbour>(8, Allocator.Persistent);
        FillWithNeighbourOffsets(ref neighbours);
    }

    // Simple AStar job
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

        // Declare values we need
        var navMap = GetSingleton<NavMap>();
        var count = navMap.Count;
        var nodeSize = navMap.NodeSize;
        var mapSize = navMap.Size;
        var transform = navMap.Transform;
        var ptr = navMap.NodesPtr;
        var neighbourRef = neighbours;

        // Execute job
        inputDeps = Entities
            .WithNone<Waypoint>()
            .WithNativeDisableUnsafePtrRestriction(ptr)
            .ForEach((Entity entity, int entityInQueryIndex, ref PathRequest req, ref Translation translation) =>
            {
                var openList = new NativeList<int>(Allocator.Temp);
                var closeList = new NativeList<int>(Allocator.Temp);
                var costs = new NativeArray<Cost>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var cameFrom = new NativeArray<int>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                for (int i = 0; i < count; i++)
                {
                    costs[i] = new Cost { G = float.MaxValue, H = 0 };
                    cameFrom[i] = -1;
                }

                // Currently only uses X & Z coordinates (no Y-axis movement)
                int endIdx = NavMap.GetIndex(new float3(req.To.x, translation.Value.y, req.To.z), nodeSize, mapSize);
                int startIdx = NavMap.GetIndex(translation.Value, nodeSize, mapSize);

                var startCost = costs[startIdx];
                startCost.G = 0;
                costs[startIdx] = startCost;

                openList.Add(startIdx);

                while (openList.Length > 0)
                {
                    int currentIdx = FindNextNode(ref openList, ref costs);
                    var currentNode = ptr[currentIdx];

                    // Destination is reached!
                    if (currentIdx == endIdx) break;

                    // Remove current node from Open List
                    for (int i = 0; i < openList.Length; i++)
                    {
                        if (openList[i] == currentIdx)
                        {
                            openList.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    // Mark current as visisted
                    closeList.Add(currentIdx);

                    for (int i = 0; i < neighbourRef.Length; ++i)
                    {
                        var neighbourOffset = neighbourRef[i].Offset;
                        var neighbourCoord = new int3(currentNode.Coord.x + neighbourOffset.x, currentNode.Coord.y, currentNode.Coord.z + neighbourOffset.y);
                        var neighbourDst = neighbourRef[i].Distance;

                        // If node is outside of map
                        if (!NavMap.NotOutOfBounds(neighbourCoord, mapSize)) continue;

                        int neighbourIdx = NavMap.GetIndex(neighbourCoord, mapSize);
                        NavMapNode neighbourNode = ptr[neighbourIdx];

                        // If node is not walkable or in closed set (already searched)
                        if (closeList.Contains(neighbourIdx) || !neighbourNode.Walkable) continue;

                        var astarCost = new Cost(costs[currentIdx].G, Heuristic(currentNode.Coord, neighbourNode.Coord));

                        if (astarCost.F < costs[neighbourIdx].G)
                        {
                            cameFrom[neighbourIdx] = currentIdx;
                            costs[neighbourIdx].Set(astarCost.F, costs[neighbourIdx].H);

                            var neighbourCost = costs[neighbourIdx];
                            neighbourCost.G = astarCost.F;
                            costs[neighbourIdx] = neighbourCost;

                            if (!openList.Contains(neighbourIdx))
                            {
                                openList.Add(neighbourIdx);
                            }
                        }
                    }
                }

                var cursor = endIdx;
                if (cameFrom[cursor] != -1)
                {
                    // We Found a path
                    var waypoints = cmdBuffer.AddBuffer<Waypoint>(entityInQueryIndex, entity);
                    while (cameFrom[cursor] != -1)
                    {
                        var coord = ptr[cursor].Coord;

                        // Use exact requested position for end node
                        // TODO: this may not be a good idea
                        var worldPos = cursor == endIdx ? req.To : transform.GetWorldPos(NavMap.ToCenterPos(coord, nodeSize));

                        var pathPoint = new float3(worldPos.x, translation.Value.y, worldPos.z);

                        waypoints.Add(new Waypoint() { Value = pathPoint });
                        cursor = cameFrom[cursor];
                    }
                }

                cmdBuffer.RemoveComponent<PathRequest>(entityInQueryIndex, entity);

                openList.Dispose();
                closeList.Dispose();
                costs.Dispose();
                cameFrom.Dispose();
            }).Schedule(inputDeps);

        cmdBufferSystem.AddJobHandleForProducer(inputDeps);

        inputDeps.Complete();

        return inputDeps;
    }

    protected override void OnDestroy()
    {
        neighbours.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindNextNode(ref NativeList<int> openList, ref NativeArray<Cost> costs)
    {
        var lowestCost = costs[openList[0]];
        var lowstCostIdx = openList[0];

        for (int i = 1; i < openList.Length; i++)
        {
            var tmp = costs[openList[i]];
            if (tmp.F < lowestCost.F)
            {
                lowestCost = tmp;
                lowstCostIdx = openList[i];
            }
        }

        return lowstCostIdx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillWithNeighbourOffsets(ref NativeArray<Neighbour> neighbourOffsetArray)
    {
        neighbourOffsetArray[0] = new Neighbour { Offset = new int2(-1, 0), Distance = 1f }; // Left
        neighbourOffsetArray[1] = new Neighbour { Offset = new int2(+1, 0), Distance = 1f }; // Right
        neighbourOffsetArray[2] = new Neighbour { Offset = new int2(0, +1), Distance = 1f }; // Up
        neighbourOffsetArray[3] = new Neighbour { Offset = new int2(0, -1), Distance = 1f }; // Down
        neighbourOffsetArray[4] = new Neighbour { Offset = new int2(-1, -1), Distance = 1.44f }; // Left Down
        neighbourOffsetArray[5] = new Neighbour { Offset = new int2(-1, +1), Distance = 1.44f }; // Left Up
        neighbourOffsetArray[6] = new Neighbour { Offset = new int2(+1, -1), Distance = 1.44f }; // Right Down
        neighbourOffsetArray[7] = new Neighbour { Offset = new int2(+1, +1), Distance = 1.44f }; // Right Up
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Heuristic(int3 from, int3 to)
    {
        /* Euclidean distance */
        var dx = math.abs(from.x - to.x);
        var dz = math.abs(from.z - to.z);
        return 1 * math.sqrt(dx * dx + dz * dz);
    }

    private struct Neighbour
    {
        public int2 Offset;
        public float Distance;
    }

    private struct Cost
    {
        public Cost(float G, float H)
        {
            this.G = G;
            this.H = H;
        }

        public float G;
        public float H;
        public float F => H + G;

        public void Set(float G, float H)
        {
            this.G = G;
            this.H = H;
        }
    }
}