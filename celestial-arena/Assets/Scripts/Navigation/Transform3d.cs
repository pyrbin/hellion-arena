using UnityEngine;

namespace Unity.Mathematics
{
    public struct transform3d
    {
        public transform3d(Transform other)
        {
            this.scale = other.localScale;
            this.transform.pos = other.position;
            this.transform.rot = other.rotation;
            this.localToWorldMatrix = other.localToWorldMatrix;
            this.worldToLocalMatrix = other.worldToLocalMatrix;
        }

        public transform3d(float3 position, quaternion rotation, float3 scale)
        {
            this.scale = scale;
            this.transform.pos = position;
            this.transform.rot = rotation;
            this.localToWorldMatrix = math.mul(new float4x4(transform), float4x4.Scale(scale));
            this.worldToLocalMatrix = math.inverse(localToWorldMatrix);
        }

        private RigidTransform transform;
        private float3 scale;
        private Matrix4x4 localToWorldMatrix;
        private Matrix4x4 worldToLocalMatrix;

        public float3 LocalPosition => transform.pos;
        public float3 LocalScale => scale;
        public quaternion LocalRotation => transform.rot;

        public Matrix4x4 ToWorldMatrix => localToWorldMatrix;
        public Matrix4x4 ToLocalMatrix => worldToLocalMatrix;

        public float3 GetWorldPos(float3 localPosition)
        {
            return math.transform(localToWorldMatrix, localPosition);
        }

        public float3 GetLocalPos(float3 worldPosition)
        {
            return math.transform(worldToLocalMatrix, worldPosition);
        }
    }
}