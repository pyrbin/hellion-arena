using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("Navigation/Nav Map")]
public class NavMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Map Settings")]
    [Comment("Navigation Map only works in positive coordinate space!", CommentType.Warning)]
    public float NodeSize;

    public int3 Size;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NavMapSystems.Build.BuildRequest
        {
            Size = Size,
            NodeSize = NodeSize,
            Transform = new transform3d(transform)
        });
    }
}
