using Unity.Entities;

namespace EcsTweens
{
    [InternalBufferCapacity(1)]
    public struct HasFloatElement : IBufferElementData
    {
        public Entity Entity;
    }
}
