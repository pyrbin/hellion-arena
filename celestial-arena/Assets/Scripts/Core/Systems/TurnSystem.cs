using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using UnityEngine;

/// <summary>
/// TurnSystem
/// </summary>
[AlwaysSynchronizeSystem]
public class TurnSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        // on create ...
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.DeltaTime;

        Entities
            .ForEach((Entity entt) =>
            {
                // system logic ...
            })
            .Run();

        return default;
    }
    
    protected override void OnDestroy()
    {
        // on destroy ...
    }
}
