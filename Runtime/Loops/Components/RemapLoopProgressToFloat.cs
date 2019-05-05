using Unity.Entities;
using Unity.Mathematics;

namespace EcsTweens
{
    public enum EasingType : byte
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad
    }

    public struct RemapLoopProgressToFloat : IComponentData
    {
        public float2 ValueBorders;
        public bool IsYoyoTweening;
        public byte Easing;

        public RemapLoopProgressToFloat(float atStart, float atEnd, bool isYoyo, EasingType easing)
        {
            ValueBorders = new float2(atStart, atEnd);
            IsYoyoTweening = isYoyo;
            Easing = (byte) easing;
        }
    }
}
