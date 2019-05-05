using Unity.Entities;

namespace EcsTweens
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(ApplyValuesSystemGroup))]
    public class TweensSyncValueBarrier : EntityCommandBufferSystem { }
}
