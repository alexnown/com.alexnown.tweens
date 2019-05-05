using Unity.Entities;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(ApplyValuesSystemGroup))]
    public abstract class ASetTweenedValueSystem <T>: ComponentSystem where T : Component
     {
        private EntityQuery _renderersWithAlpha;
        private EntityQueryBuilder.F_CD<T, FloatContainer> _cachedForEach;

        protected override void OnCreate()
        {
            _renderersWithAlpha = GetEntityQuery(
                ComponentType.ReadOnly<T>(),
                ComponentType.ReadOnly<FloatContainer>());
        }

        protected override void OnUpdate()
        {
            Entities.With(_renderersWithAlpha).ForEach(_cachedForEach = _cachedForEach ?? ApplyTweenValue);
        }

        protected abstract void ApplyTweenValue(T renderer, ref FloatContainer value);
     }
}