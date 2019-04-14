using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace EcsTweens
{
    [UpdateInGroup(typeof(ApplyValuesSystemGroup))]
    public class ApplyAlphaToSpritesSystem : ComponentSystem
    {
        private EntityQuery _renderersWithAlpha;

        protected override void OnCreate()
        {
            _renderersWithAlpha = GetEntityQuery(ComponentType.ReadOnly<SpriteRenderer>(),
                ComponentType.ReadOnly<FloatContainerAsAlpha>(), ComponentType.ReadOnly<FloatContainer>());
            _renderersWithAlpha.SetFilterChanged(ComponentType.ReadOnly<FloatContainer>());
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample(nameof(ApplyAlphaToSpritesSystem));
            Entities.With(_renderersWithAlpha).ForEach((SpriteRenderer renderer, ref FloatContainer value) =>
            {
                var color = renderer.color;
                color.a = value.Value;
                renderer.color = color;
            });
            Profiler.EndSample();
        }
    }
}
