using Unity.Entities;

namespace EcsTweens
{
    /// <summary>
    /// Number of remaining loops. Tween will end when reach bounds if loops number less 2.
    /// </summary>
    public struct TweenLoops : IComponentData
    {
        public uint Value;
    }
}
