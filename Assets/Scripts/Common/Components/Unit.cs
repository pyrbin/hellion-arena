using Unity.Entities;
using UnityEngine;

/// <summary>
/// Unit
/// </summary>
[GenerateAuthoringComponent]
public struct Unit : IComponentData
{
    public float Movespeed;
}
