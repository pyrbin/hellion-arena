using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public static class PositionUtil
{
    public static float3 ZeroY(float3 coord)
    {
        return new float3(coord.x, 0, coord.z);
    }
}