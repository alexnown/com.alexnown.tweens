using Unity.Entities;

namespace EcsTweens
{
    [UpdateAfter(typeof(TweenSystemGroup))]
    public class TweensSyncValueBarrier : EntityCommandBufferSystem {}
}
