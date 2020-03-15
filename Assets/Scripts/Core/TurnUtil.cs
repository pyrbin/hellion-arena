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
    {
        em.CreateEntity(typeof(TurnBeginSystem.TriggerComponent));
    }

    public static void EndTurn(EntityManager em)
    {
        em.CreateEntity(typeof(TurnEndSystem.TriggerComponent));
    }

    public static Entity CurrentTurnEntity(ComponentSystemBase caller)
    {
        return TurnQueueBuffer(caller)[0].Entity;
    }

    public static DynamicBuffer<TurnQueue> TurnQueueBuffer(ComponentSystemBase caller)
    {
        return caller.GetBufferFromEntity<TurnQueue>()[caller.GetSingletonEntity<TurnQueue>()];
    }
}