using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using UnityEngine;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
public class TurnUpdateSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<TurnQueue>();
    }

    protected override void OnUpdate()
    {
        Entities
            .WithAll<Actor>()
            .ForEach((Entity entity) =>
            {
                if (!TurnUtil.ActorInQueue(entity, this))
                    TurnUtil.AddActorToQueue(entity, this);
            });
    }
}