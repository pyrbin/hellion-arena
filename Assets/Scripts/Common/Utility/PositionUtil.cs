using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public static class PositionUtil
{
    public static float3 SetY(float3 coord, float y = 0)
    {
        return new float3(coord.x, y, coord.z);
    }

    public static bool ReachedPos(float3 from, float3 to, float offset = 0.1f)
    {
        return math.length((from - to).xz) > offset;
    }
}
