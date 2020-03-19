using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public static class MovementOrder
{
    public struct Request : IComponentData
    {
        public float3 To;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public unsafe class HandleRequest : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem Barrier;
        private NativeArray<AStarPathfinding.Neighbour> Neighbour;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<NavMap>();
            Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            Neighbour = AStarPathfinding.Neighbour.Create(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            AStarPathfinding.Neighbour.Destroy(ref Neighbour);
        }

        // Simple AStar job
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cmdBuffer = Barrier.CreateCommandBuffer().ToConcurrent();

            // Declare values we need
            var navMap = GetSingleton<NavMap>();
            var transform = navMap.Transform;
            var nodePtr = navMap.NodesPtr;
            var neighboursRef = Neighbour;

            // Execute job
            inputDeps = Entities
                .WithAll<NavAgent>()
                .WithNone<Waypoint>()
                .ForEach((Entity entity, int entityInQueryIndex, Request req, ref Translation translation) =>
                {
                    // Consume request component
                    cmdBuffer.RemoveComponent<Request>(entityInQueryIndex, entity);

                    // Try find path using AStar
                    if (!AStarPathfinding.Solve(translation.Value, req.To, ref navMap, ref neighboursRef, out var path, out var cursor)) return;

                    if (path[cursor] != -1)
                    {
                        // We Found a path
                        var waypoints = cmdBuffer.AddBuffer<Waypoint>(entityInQueryIndex, entity);
                        while (path[cursor] != -1)
                        {
                            var coord = navMap.NodesPtr[cursor].Coord;
                            var worldPos = transform.GetWorldPos(NavMap.ToCenterPos(coord, navMap.NodeSize));
                            var pathPoint = new float3(worldPos.x, translation.Value.y, worldPos.z);
                            waypoints.Add(new Waypoint() { Value = pathPoint });
                            cursor = path[cursor];
                        }
                    }

                    path.Dispose();
                }).Schedule(inputDeps);

            Barrier.AddJobHandleForProducer(inputDeps);

            inputDeps.Complete();

            return inputDeps;
        }
    }

    // AStarPathfinding
    // TODO: Add more advanced pathfinding, avoidance, agent size/height, 3D navigation
    public unsafe struct AStarPathfinding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Solve(float3 from, float3 to, ref NavMap map, ref NativeArray<Neighbour> neighbourOffsets, out NativeArray<int> cameFrom, out int firstIndex)

        {
            // Currently only uses X & Z coordinates (no Y-axis movement)
            int endIdx = NavMap.GetIndex(PositionUtil.SetY(to, from.y), map.NodeSize, map.Size);
            int startIdx = NavMap.GetIndex(from, map.NodeSize, map.Size);

            // Allocate path list
            cameFrom = new NativeArray<int>(map.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            firstIndex = startIdx;

            // If destination node is invalid skip AStar search.
            if (!NavMap.NotOutOfBounds(map.NodesPtr[endIdx].Coord, map.Size) || !map.NodesPtr[endIdx].Walkable) return false;

            var openList = new NativeList<int>(Allocator.Temp);
            var closeList = new NativeList<int>(Allocator.Temp);
            var costs = new NativeArray<Cost>(map.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // Initial values
            for (int i = 0; i < map.Count; i++)
            {
                costs[i] = new Cost { G = float.MaxValue, H = 0 };
                cameFrom[i] = -1;
            }

            var startCost = costs[startIdx];
            startCost.G = 0;
            costs[startIdx] = startCost;

            openList.Add(startIdx);

            while (openList.Length > 0)
            {
                int currentIdx = FindNextNode(ref openList, ref costs);
                var currentNode = map.NodesPtr[currentIdx];

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

                // Mark current as visited
                closeList.Add(currentIdx);

                for (int i = 0; i < neighbourOffsets.Length; ++i)
                {
                    var neighbourOffset = neighbourOffsets[i].Offset;
                    var neighbourCoord = new int3(currentNode.Coord.x + neighbourOffset.x, currentNode.Coord.y, currentNode.Coord.z + neighbourOffset.y);
                    var neighbourDst = neighbourOffsets[i].Distance;

                    // If node is outside of map
                    if (!NavMap.NotOutOfBounds(neighbourCoord, map.Size)) continue;

                    int neighbourIdx = NavMap.GetIndex(neighbourCoord, map.Size);
                    var neighbourNode = map.NodesPtr[neighbourIdx];

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

            firstIndex = endIdx;

            openList.Dispose();
            closeList.Dispose();
            costs.Dispose();

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindNextNode(ref NativeList<int> openList, ref NativeArray<Cost> costs)
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
        public static float Heuristic(int3 from, int3 to)
        {
            /* Euclidean distance */
            var dx = math.abs(from.x - to.x);
            var dz = math.abs(from.z - to.z);
            return 1 * math.sqrt(dx * dx + dz * dz);
        }

        public struct Neighbour
        {
            public int2 Offset;
            public float Distance;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static NativeArray<Neighbour> Create(Allocator allocator)
            {
                var neighbours = new NativeArray<Neighbour>(8, allocator);
                neighbours[0] = new Neighbour { Offset = new int2(-1, 0), Distance = 1f }; // Left
                neighbours[1] = new Neighbour { Offset = new int2(+1, 0), Distance = 1f }; // Right
                neighbours[2] = new Neighbour { Offset = new int2(0, +1), Distance = 1f }; // Up
                neighbours[3] = new Neighbour { Offset = new int2(0, -1), Distance = 1f }; // Down
                neighbours[4] = new Neighbour { Offset = new int2(-1, -1), Distance = 1.44f }; // Left Down
                neighbours[5] = new Neighbour { Offset = new int2(-1, +1), Distance = 1.44f }; // Left Up
                neighbours[6] = new Neighbour { Offset = new int2(+1, -1), Distance = 1.44f }; // Right Down
                neighbours[7] = new Neighbour { Offset = new int2(+1, +1), Distance = 1.44f }; // Right Up
                return neighbours;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Destroy(ref NativeArray<Neighbour> array)
            {
                array.Dispose();
            }
        }

        public struct Cost
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
}
