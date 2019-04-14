using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace EcsTweens
{
    [UpdateInGroup(typeof(ApplyValuesSystemGroup))]
    public class ApplyAlphaToRemoteSpritesSystem : ComponentSystem
    {
        private EntityQuery _remoteRenderers;

        protected override void OnCreate()
        {
            _remoteRenderers = GetEntityQuery(ComponentType.Exclude<SpriteRenderer>(),
                ComponentType.ReadOnly<FloatContainerAsAlpha>(), ComponentType.ReadOnly<FloatContainer>());
            _remoteRenderers.SetFilterChanged(ComponentType.ReadOnly<FloatContainer>());
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample(nameof(ApplyAlphaToRemoteSpritesSystem));
            Entities.With(_remoteRenderers).ForEach((Entity e, ref FloatContainer value, ref FloatContainerAsAlpha target) =>
            {
                bool hasRenderer = EntityManager.HasComponent<SpriteRenderer>(target.TargetRenderer);
                if (hasRenderer)
                {
                    var renderer = EntityManager.GetComponentObject<SpriteRenderer>(target.TargetRenderer);
                    var color = renderer.color;
                    color.a = value.Value;
                    renderer.color = color;
                }
                else PostUpdateCommands.DestroyEntity(e);
            });
            Profiler.EndSample();
        }
    }
}
