using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public static class TurnUtil
{
    public enum TurnState : byte
    {
        Proccess
    }

    public static void NextTurn(EntityManager em)
        => em.CreateEntity(typeof(TurnBeginSystem.TriggerComponent));

    public static void AddActorToQueue(Entity entity, ComponentSystemBase caller)
        => TurnQueueBuffer(caller).Add(new TurnQueue { Entity = entity });

    public static bool AnyInQueue(ComponentSystemBase caller)
        => TurnQueueBuffer(caller).Length > 0;

    public static bool ActorInQueue(Entity entity, ComponentSystemBase caller)
        => new SingleShotEnumerable<TurnQueue>(TurnQueueBuffer(caller).GetEnumerator()).Any(x => x.Entity == entity);

    public static void EndTurn(EntityManager em)
        => em.CreateEntity(typeof(TurnEndSystem.TriggerComponent));

    public static Entity CurrentTurnEntity(ComponentSystemBase caller)
        => TurnQueueBuffer(caller)[0].Entity;

    public static DynamicBuffer<TurnQueue> TurnQueueBuffer(ComponentSystemBase caller)
        => caller.GetBufferFromEntity<TurnQueue>()[caller.GetSingletonEntity<TurnQueue>()];
}