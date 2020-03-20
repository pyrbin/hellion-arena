using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MoveUnitSystem : ComponentSystem
{
    private uint Tick
        => World.GetExistingSystem<GhostPredictionSystemGroup>().PredictingTick;

    protected override void OnUpdate()
    {
        var dt = Time.fixedDeltaTime;

        // Execute job
        Entities
            .ForEach((DynamicBuffer<UserCmd> cmdBuffer, ref PredictedGhostComponent prediction, ref Translation translation, ref NavAgent agent) =>
            {
                if (!GhostPredictionSystemGroup.ShouldPredict(Tick, prediction))
                    return;

                cmdBuffer.GetDataAtTick(Tick, out var cmd);

                // UserCmd.Action.MoveOrder
                if (cmd.Actions.Has(UserCmd.Action.MoveOrder))
                {
                    var stepSize = agent.Speed * dt;
                    var direction = cmd.moveOrderTo - translation.Value;

                    if (math.length(direction.xz) > 0.1f)
                    {
                        direction = math.normalize(direction);
                        translation.Value.xz += direction.xz * stepSize;
                    }
                }
            });
    }
}
