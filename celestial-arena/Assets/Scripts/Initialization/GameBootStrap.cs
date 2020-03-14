using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Scenes;

#if UNITY_EDITOR

using UnityEditor;
using Unity.NetCode.Editor;

#endif

public class GameBootStrap : ClientServerBootstrap
{
    public static World DefaultWorld => World.DefaultGameObjectInjectionWorld;

    public override bool Initialize(string defaultWorldName)
    {
        World.DefaultGameObjectInjectionWorld = new World(defaultWorldName);

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(DefaultWorld, systems);
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(DefaultWorld);

        return true;
    }
}