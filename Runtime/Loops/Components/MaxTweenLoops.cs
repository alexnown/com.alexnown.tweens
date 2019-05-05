using Unity.Entities;

namespace EcsTweens
{
    /// <summary>
    /// Number of remaining loops. Tween will end when complited loops count >= max loops count.
    /// </summary>
    public struct MaxTweenLoops : IComponentData
    {
        public ushort Value;
    }
}
