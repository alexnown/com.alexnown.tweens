using Unity.Entities;

namespace EcsTweens
{
    [InternalBufferCapacity(1)]
    public struct TweenTargetElement : IBufferElementData
    {
        public Entity Entity;
    }
}
