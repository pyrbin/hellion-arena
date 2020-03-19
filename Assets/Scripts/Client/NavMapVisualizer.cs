using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NavMapVisualizer : MonoBehaviour
{
    public bool Enable = true;
    public bool WireCubes = false;

    public Color NormalColor;
    public Color UnwalkableColor;

    public bool HasSpawned(out NavMap? navmap)
    {
        navmap = null;
        var world = WorldUtil.ClientWorld;

        if (world == null) return false;

        var em = world.EntityManager;

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

            var centerPos = navmap.Transform.GetWorldPos(NavMap.ToCenterPos(node.Coord, navmap.NodeSize));

            if (WireCubes) Gizmos.DrawWireCube(centerPos, new float3(navmap.NodeSize, 0, navmap.NodeSize));
            else Gizmos.DrawCube(centerPos, new float3(navmap.NodeSize, 0, navmap.NodeSize));
        }
    }
}
