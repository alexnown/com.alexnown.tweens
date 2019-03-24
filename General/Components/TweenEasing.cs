using Unity.Entities;

namespace EcsTweens
{
    public enum EasingType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad
    }

    public struct TweenEasing : IComponentData
    {
        public byte Type;

        public TweenEasing(EasingType easing)
        {
            Type = (byte)easing;
        }
    }
}
