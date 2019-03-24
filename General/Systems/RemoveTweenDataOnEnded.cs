using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class RemoveTweenDataOnEnded : JobComponentSystem
    {
        struct RemoveTweenComponentsIfEnded : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntitieType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenBounds> BoundsType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenComplitedState> ComplitedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenEasing> EasingType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenFloatTarget> TargetType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenSpeed> SpeedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenYoyoDirection> DirectionType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntitieType);
                var states = chunk.GetNativeArray(ComplitedType);
                bool hasBounds = chunk.Has(BoundsType);
                bool hasEasing = chunk.Has(EasingType);
                bool hasTarget = chunk.Has(TargetType);
                bool hasProgress = chunk.Has(ProgressType);
                bool hasSpeed = chunk.Has(SpeedType);
                bool hasDirection = chunk.Has(DirectionType);

                for (int i = 0; i < states.Length; i++)
                {
                    if (!states[i].IsComplited) continue;
                    var e = entities[i];
                    Cb.RemoveComponent<TweenComplitedState>(chunkIndex, e);
                    if (hasBounds) Cb.RemoveComponent<TweenBounds>(chunkIndex, e);
                    if (hasEasing) Cb.RemoveComponent<TweenEasing>(chunkIndex, e);
                    if (hasTarget) Cb.RemoveComponent<TweenFloatTarget>(chunkIndex, e);
                    if (hasProgress) Cb.RemoveComponent<TweenProgress>(chunkIndex, e);
                    if (hasSpeed) Cb.RemoveComponent<TweenSpeed>(chunkIndex, e);
                    if (hasDirection) Cb.RemoveComponent<TweenYoyoDirection>(chunkIndex, e);
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endBarrier;
        private ComponentGroup _tweens;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _tweens = GetComponentGroup(ComponentType.ReadOnly<TweenComplitedState>(), ComponentType.Exclude<DestroyOnComplite>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var checkTweenEndedJob = new RemoveTweenComponentsIfEnded
            {
                Cb = _endBarrier.CreateCommandBuffer().ToConcurrent(),
                EntitieType = GetArchetypeChunkEntityType(),
                SpeedType = GetArchetypeChunkComponentType<TweenSpeed>(true),
                ComplitedType = GetArchetypeChunkComponentType<TweenComplitedState>(true),
                ProgressType = GetArchetypeChunkComponentType<TweenProgress>(true),
                TargetType = GetArchetypeChunkComponentType<TweenFloatTarget>(true),
                EasingType = GetArchetypeChunkComponentType<TweenEasing>(true),
                DirectionType = GetArchetypeChunkComponentType<TweenYoyoDirection>(true),
                BoundsType = GetArchetypeChunkComponentType<TweenBounds>(true)
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(checkTweenEndedJob);
            return checkTweenEndedJob;
        }
    }
}
