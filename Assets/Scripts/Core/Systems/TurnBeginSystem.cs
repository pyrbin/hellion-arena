using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// TurnBeginSystem
/// </summary>
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TurnBeginSystem : JobComponentSystem
{
    public struct TriggerComponent : IComponentData { }

    private EntityQuery TurnTakers;
    private EntityQuery TurnTakerActions;

    protected override void OnCreate()
    {
        // Require Turn Start event component
        RequireSingletonForUpdate<TriggerComponent>();

        // Require a queue of potential turn actors
        RequireSingletonForUpdate<TurnQueue>();

        TurnTakers = GetEntityQuery(
            ComponentType.ReadOnly<Actor>(),
            ComponentType.ReadOnly<TurnActor>()
        );
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Don't start any new turns until current turns are complete
        if (!TurnTakers.IsEmptyIgnoreFilter)
            return default;

        // Start turn first actor in queue
        BeginTurn(out var entity);
        Debug.Log("Start-Turn for entity: " + entity);
        // ...............................

        return default;
    }

    public void BeginTurn(out Entity entity)
    {
        entity = TurnUtil.CurrentTurnEntity(this);
        EntityManager.DestroyEntity(GetSingletonEntity<TurnBeginSystem.TriggerComponent>());
        EntityManager.AddComponent<TurnActor>(entity);
    }

    protected override void OnDestroy()
    {
        // on destroy ...
    }
}