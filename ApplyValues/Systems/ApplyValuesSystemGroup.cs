using Unity.Entities;

namespace EcsTweens
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class ApplyValuesSystemGroup : ComponentSystemGroup { }
}
