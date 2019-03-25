using Unity.Entities;

namespace EcsTweens
{
    public struct TweenBounds : IComponentData
    {
        public float Min;
        public float Max;
    }
}
