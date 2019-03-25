using Unity.Entities;

namespace EcsTweens
{
    /// <summary>
    /// Used in loop tweens as progress value in range [0, 1].
    /// </summary>
    public struct TweenProgress : IComponentData
    {
        public float Value;
    }
}
