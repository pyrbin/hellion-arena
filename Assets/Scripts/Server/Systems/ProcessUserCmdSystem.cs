using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// MoveOrderCmdSystem
/// </summary>
[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class ProcessUserCmdSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        // net-code data
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;

        // Execute job
        Entities
            .ForEach((Entity entity, DynamicBuffer<UserCmd> cmdBuffer, ref PredictedGhostComponent prediction) =>
            {
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                    return;

                cmdBuffer.GetDataAtTick(tick, out var cmd);

                // If UserCommand has MoveOrder
                if (cmd.Actions.Has(UserCmd.Action.MoveOrder))
                {
                    EntityManager.AddComponentData(entity, new MovementOrder.Request { To = cmd.moveOrderPos });
                }
            });
    }
}
