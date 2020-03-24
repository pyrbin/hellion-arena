using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.NetCode;
using Unity.Mathematics;
using UnityEngine;

public static class TurnCoordinator
{
    public struct ActiveTurn : IComponentData { }

    [InternalBufferCapacity(8)]
    public struct TurnQueue : IBufferElementData
    {
        public Entity Entity;
    }

    public struct InTurnQueue : IComponentData { }

    [RequireComponent(typeof(TurnQueue))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GhostSimulationSystemGroup))]
    [UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
    public class TurnSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
        }
    }

    [UpdateInGroup(typeof(TurnSystemGroup))]
    public class AddToQueue : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Unit>()
                .WithNone<InTurnQueue>()
                .WithStructuralChanges()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<InTurnQueue>(entity);
                    TurnQueueBuffer(this).Add(new TurnQueue { Entity = entity });
                    Debug.Log("Adding to Queue :)");
                })
                .Run();
        }
    }

    /*
    [UpdateInGroup(typeof(TurnSystemGroup))]
    [UpdateBefore(typeof(TurnGameplaySystemGroup))]
    public class BeginTurnSystem : SystemBase
    {
        public struct StartComponent : IComponentData { }

        public EntityQuery ActiveTurnUnit;

        protected override void OnCreate()
        {
            // Require Turn Start event component
            RequireSingletonForUpdate<StartComponent>();

            ActiveTurnUnit
                = GetEntityQuery(ComponentType.ReadOnly<ActiveTurn>());
        }

        protected override void OnUpdate()
        {
            // Don't start any new turns until current turns are complete
            if (!ActiveTurnUnit.IsEmptyIgnoreFilter) return;

            TryBeginTurn(out var entity);

            Debug.Log("Start-Turn for entity: " + entity);
        }

        public void TryBeginTurn(out Entity entity)
        {
            entity = ActiveTurnEntity(this);
            EntityManager.DestroyEntity(GetSingletonEntity<StartComponent>());
            EntityManager.AddComponent<ActiveTurn>(entity);
        }
    }

    [UpdateInGroup(typeof(TurnSystemGroup))]
    public class TurnGameplaySystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ActiveTurn>();
        }
    }

    [UpdateInGroup(typeof(TurnSystemGroup))]
    [UpdateAfter(typeof(TurnGameplaySystemGroup))]
    public class EndTurnSystem : SystemBase
    {
        public struct EndComponent : IComponentData { }

        protected override void OnCreate()
        {
            // Require Turn Start event component
            RequireSingletonForUpdate<EndComponent>();
        }

        protected override void OnUpdate()
        {
            if (TryEndTurn(out var entity))
            {
                Debug.Log("End-Turn for entity: " + entity);
                CreateTurnStartComponent(EntityManager);
            }
        }

        public bool TryEndTurn(out Entity entity)
        {
            // Insert current entity at the end & remove from the start.
            entity = ActiveTurnEntity(this);
            EntityManager.RemoveComponent<ActiveTurn>(entity);

            var queue = TurnQueueBuffer(this);
            queue.Add(new TurnQueue { Entity = entity });

            TurnQueueBuffer(this).RemoveAt(0);
            EntityManager.DestroyEntity(GetSingletonEntity<EndComponent>());
            return true;
        }
    }

    public static void CreateTurnStartComponent(EntityManager em)
    {
        em.CreateEntity(typeof(BeginTurnSystem.StartComponent));
    }

    public static void CreateTurnEndComponent(EntityManager em)
    {
        em.CreateEntity(typeof(EndTurnSystem.EndComponent));
    }
      */

    public static Entity ActiveTurnEntity(ComponentSystemBase caller)
    {
        return TurnQueueBuffer(caller)[0].Entity;
    }

    public static DynamicBuffer<TurnQueue> TurnQueueBuffer(ComponentSystemBase caller)
    {
        return caller.GetBufferFromEntity<TurnQueue>()[caller.GetSingletonEntity<TurnQueue>()];
    }
}
