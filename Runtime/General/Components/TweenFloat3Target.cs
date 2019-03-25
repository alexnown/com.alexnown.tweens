using Unity.Entities;
using Unity.Mathematics;

namespace EcsTweens
{
    public struct TweenFloat3Target : IComponentData
    {
        public float3 Value;
    }
}
