using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using UnityEngine;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
public class TurnInitSystem : ComponentSystem
{
    private struct InitTurnComponent : IComponentData { }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitTurnComponent>();
        EntityManager.CreateEntity(typeof(InitTurnComponent));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(GetSingletonEntity<InitTurnComponent>());

        Debug.Log("Init-Turn systems");

        var queueEntity = EntityManager.CreateEntity();
        var queue = EntityManager.AddBuffer<TurnQueue>(queueEntity);

        Entities
            .WithAll<Actor>()
            .ForEach((Entity entity) =>
            {
                queue.Add(new TurnQueue { Entity = entity });
            });

        // TurnUtil.NextTurn(EntityManager);
    }
}