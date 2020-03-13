using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[ExecuteInEditMode]
public class NavMapVisualizer : MonoBehaviour
{
    public bool Enable = true;
    public bool WireCubes = false;

    public Color NormalColor;
    public Color UnwalkableColor;

    public bool HasSpawned(out NavMap? navmap)
    {
        navmap = null;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var q = em.CreateEntityQuery(typeof(NavMap));
        if (q.CalculateEntityCount() <= 0) return false;
        navmap = q.GetSingleton<NavMap>();
        return true;
    }

    public unsafe void OnDrawGizmos()
    {
        if (!Enable) return;
        NavMap? fetch;

        if (!HasSpawned(out fetch)) return;

        var navmap = (NavMap)fetch;

        foreach (var node in navmap.Nodes.ToArray())
        {
            Gizmos.matrix = navmap.Transform.ToWorldMatrix;
            Gizmos.color = node.Walkable ? NormalColor : UnwalkableColor;

            var centerPos = navmap.Transform.GetWorldPos(NavMap.ToCenterPos(node.Coord, navmap.TileSize));

            if (WireCubes) Gizmos.DrawWireCube(centerPos, new float3(navmap.TileSize, 0, navmap.TileSize));
            else Gizmos.DrawCube(centerPos, new float3(navmap.TileSize, 0, navmap.TileSize));
        }
    }
}