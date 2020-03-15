using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[AddComponentMenu("Navigation/Navigation Map")]
public class NavMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Map Settings")]
    [Comment("Navigation Map only works in positive coordinate space!", CommentType.Warning)]
    public float NodeSize;

    public int3 Size;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NavMapCreate
        {
            Size = Size,
            NodeSize = NodeSize,
            Transform = new transform3d(transform)
        });
    }
}