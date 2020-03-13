using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
[AddComponentMenu("Navigation/Navigation Map")]
public class NavMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float TileSize;
    public int3 Size;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NavMapCreate
        {
            Size = Size,
            TileSize = TileSize,
            Transform = new transform3d(transform)
        });
    }
}