using Unity.Entities;

namespace EcsTweens
{
    /// <summary>
    /// Target float value for tween. Tween will end when FloatContainer reach this value. 
    /// </summary>
    public struct TweenFloatTarget : IComponentData
    {
        public float Value;
    }
}
