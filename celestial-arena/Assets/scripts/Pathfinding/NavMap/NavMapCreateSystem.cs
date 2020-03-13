using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
[AlwaysSynchronizeSystem]
public unsafe class NavMapCreateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();
        Entities
            .WithStructuralChanges()
            .ForEach((Entity entity, ref NavMapCreate create) =>
            {
                var size = create.Size;
                var nodeSize = create.NodeSize;
                var transform = create.Transform;

                var blobBuilder = new BlobBuilder(Allocator.Temp);
                ref var navMapBlob = ref blobBuilder.ConstructRoot<NavMapBlob>();

                var nodes = blobBuilder.Allocate(ref navMapBlob.Nodes, NavMap.NodeCount(size));

                navMapBlob.Size = size;
                navMapBlob.NodeSize = nodeSize;
                navMapBlob.Transform = transform;

                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        for (int z = 0; z < size.z; z++)
                        {
                            var coord = new int3(x, y, z);
                            var centerPos = transform.GetWorldPos(NavMap.ToCenterPos(coord, nodeSize));

                            nodes[NavMap.GetIndex(coord, size)] = new NavMapNode
                            {
                                Coord = coord,
                                Walkable = true,
                                Aabb = CreateBoxAABB(centerPos, new float3(1) * nodeSize, quaternion.identity)
                            };
                        }
                    }
                }

                // Add NavMap component
                EntityManager.AddComponentData(entity, new NavMap
                {
                    Blob = blobBuilder.CreateBlobAssetReference<NavMapBlob>(Allocator.Persistent)
                });

                blobBuilder.Dispose();

                // Remove NavMap create component
                EntityManager.RemoveComponent<NavMapCreate>(entity);

                // Push NavMapBuild (build obstacles)
                EntityManager.AddComponentData(entity, new NavMapBuild());
            }).Run();

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Aabb CreateBoxAABB(float3 center, float3 size, quaternion orientation)
    {
        var geometry = new BoxGeometry
        {
            Center = center,
            Orientation = orientation,
            Size = size,
        };

        Unity.Physics.RigidBody rigidbodyBox = Unity.Physics.RigidBody.Zero;
        rigidbodyBox.Collider = (Unity.Physics.Collider*)Unity.Physics.BoxCollider.Create(geometry).GetUnsafePtr();
        return rigidbodyBox.CalculateAabb();
    }
}