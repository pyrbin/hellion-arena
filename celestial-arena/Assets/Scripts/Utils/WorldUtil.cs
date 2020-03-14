using System.Linq;
using Unity.Entities;
using Unity.NetCode;

public static class WorldUtil
{
    public static World ClientWorld
        => new SingleShotEnumerable<World>(World.All.GetEnumerator())
            .FirstOrDefault(x => x.GetExistingSystem<ClientSimulationSystemGroup>() != null);

    public static World ServerWorld
        => new SingleShotEnumerable<World>(World.All.GetEnumerator())
            .FirstOrDefault(x => x.GetExistingSystem<ServerSimulationSystemGroup>() != null);
}