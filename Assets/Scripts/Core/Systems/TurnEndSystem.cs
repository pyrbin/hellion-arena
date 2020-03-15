using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// TurnBeginSystem
/// </summary>
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class TurnEndSystem : JobComponentSystem
{
    public struct TriggerComponent : IComponentData { }

    private EntityQuery TurnTakerActions0;
    private EntityQuery TurnTakerActions1;

    public bool EndingInProgress { get; private set; } = false;

    protected override void OnCreate()
    {
        // Require Turn End event component
        RequireSingletonForUpdate<TriggerComponent>();

        // Require a queue of potential turn actors
        RequireSingletonForUpdate<TurnQueue>();

        TurnTakerActions0 = GetEntityQuery(
            ComponentType.ReadOnly<Actor>(),
            ComponentType.ReadOnly<Waypoint>()
        );

        TurnTakerActions1 = GetEntityQuery(
            ComponentType.ReadOnly<Actor>(),
            ComponentType.ReadOnly<PathRequest>()
        );
    }

    protected bool TurnActionsInProgress
        => !TurnTakerActions0.IsEmptyIgnoreFilter || !TurnTakerActions1.IsEmptyIgnoreFilter;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // End turn ......................
        if (TryEndTurn(out var entity))
        {
            TurnUtil.NextTurn(EntityManager);
            Debug.Log("End-Turn for entity: " + entity);
        }
        // ...............................

        return default;
    }

    public bool TryEndTurn(out Entity entity)
    {
        // Insert current entity at the end & remove from the start.
        entity = TurnUtil.CurrentTurnEntity(this);
        if (!EndingInProgress)
        {
            EntityManager.RemoveComponent<TurnActor>(entity);
            EndingInProgress = true;
        }
        if (EndingInProgress && !TurnActionsInProgress)
        {
            var queue = TurnUtil.TurnQueueBuffer(this);
            queue.Add(new TurnQueue { Entity = entity });
            TurnUtil.TurnQueueBuffer(this).RemoveAt(0);
            EntityManager.DestroyEntity(GetSingletonEntity<TriggerComponent>());
            EndingInProgress = false;
            return true;
        }
        return false;
    }

    protected override void OnDestroy()
    {
        // on destroy ...
    }
}