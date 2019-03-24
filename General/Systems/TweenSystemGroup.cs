using Unity.Entities;

namespace EcsTweens
{
    [UpdateBefore(typeof(ApplyValuesSystemGroup))]
    public class TweenSystemGroup : ComponentSystemGroup { }
}
