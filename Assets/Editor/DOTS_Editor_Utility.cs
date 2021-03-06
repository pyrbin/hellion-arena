﻿using UnityEditor;
using UnityEngine;

public class DOTS_Editor_Utility : MonoBehaviour
{
    private const string COMPONENT_DATA_SCRIPT_PATH = "Assets/Editor/Templates/ComponentData.cs.txt";
    private const string COMPONENT_BUFFER_SCRIPT_PATH = "Assets/Editor/Templates/ComponentBuffer.cs.txt";
    private const string SIMPLE_SYSTEM_SCRIPT_PATH = "Assets/Editor/Templates/SimpleSystem.cs.txt";
    private const string SIMPLE_JOB_SYSTEM_SCRIPT_PATH = "Assets/Editor/Templates/SimpleJobSystem.cs.txt";
    private const string JOB_SYSTEM_SCRIPT_PATH = "Assets/Editor/Templates/JobSystem.cs.txt";

    [MenuItem(itemName: "Assets/Create/DOTS/Objects/Component", isValidateFunction: false, priority: 1)]
    public static void CreateComponentDataScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(COMPONENT_DATA_SCRIPT_PATH, "ExampleComponent.cs");
    }

    [MenuItem(itemName: "Assets/Create/DOTS/Objects/ComponentBuffer", isValidateFunction: false, priority: 2)]
    public static void CreateComponentBufferScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(COMPONENT_BUFFER_SCRIPT_PATH, "ExampleComponent.cs");
    }

    [MenuItem(itemName: "Assets/Create/DOTS/Objects/Simple Job System", isValidateFunction: false, priority: 3)]
    public static void CreateSimpleJobSystemScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(SIMPLE_JOB_SYSTEM_SCRIPT_PATH, "ExampleSystem.cs");
    }

    [MenuItem(itemName: "Assets/Create/DOTS/Objects/Job System", isValidateFunction: false, priority: 4)]
    public static void CreateJobSystemScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(JOB_SYSTEM_SCRIPT_PATH, "ExampleSystem.cs");
    }

    [MenuItem(itemName: "Assets/Create/DOTS/Objects/Simple System", isValidateFunction: false, priority: 5)]
    public static void CreateSimpleSystemScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(SIMPLE_SYSTEM_SCRIPT_PATH, "ExampleSystem.cs");
    }
}
