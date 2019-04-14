using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    [BurstCompile]
    [UpdateInGroup(typeof(TweenSystemGroup))]
    [UpdateAfter(typeof(UpdateLoopedTweenSystem))]
    public class RemapProgressToFloatValue : JobComponentSystem
    {
        [BurstCompile]
        struct ProgressToFloatValueJob : IJobChunk
        {
            public ArchetypeChunkComponentType<FloatContainer> ValueType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenEasing> EasingType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenYoyoDirection> YoyoDirectionType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenBounds> BoundsType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var values = chunk.GetNativeArray(ValueType);
                var tweenProgresses = chunk.GetNativeArray(ProgressType);
                var easings = chunk.GetNativeArray(EasingType);
                var yoyoDirections = chunk.GetNativeArray(YoyoDirectionType);
                var bounds = chunk.GetNativeArray(BoundsType);

                for (int i = 0; i < tweenProgresses.Length; i++)
                {
                    float progress = tweenProgresses[i].Value;
                    if (yoyoDirections.IsCreated)
                    {
                        if (yoyoDirections[i].IsBack) progress = 1 - progress;
                    }
                    if (easings.IsCreated)
                    {
                        progress = EasingValue(progress, easings[i].Type);
                    }
                    if (bounds.IsCreated)
                    {
                        var boundsData = bounds[i];
                        progress = boundsData.Min + progress * (boundsData.Max - boundsData.Min);
                    }
                    values[i] = new FloatContainer { Value = progress };
                }
            }


            private float EasingValue(float progress, byte type)
            {
                switch (type)
                {
                    case (byte)EcsTweens.EasingType.OutQuad:
                        return -progress * (progress - 2);
                    case (byte)EcsTweens.EasingType.InQuad:
                        return progress * progress;
                    case (byte)EcsTweens.EasingType.InOutQuad:
                        float ratio = progress * 2;
                        if (ratio < 1) return 0.5f * ratio * ratio;
                        ratio--;
                        return -0.5f * (ratio * (ratio - 2) - 1);
                    default:
                        return progress;
                }
            }
        }

        private EntityQuery _tweens;
        protected override void OnCreateManager()
        {
            _tweens = GetEntityQuery(ComponentType.ReadOnly<TweenProgress>(),
                ComponentType.ReadOnly<FloatContainer>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ProgressToFloatValueJob
            {
                ValueType = GetArchetypeChunkComponentType<FloatContainer>(),
                EasingType = GetArchetypeChunkComponentType<TweenEasing>(true),
                BoundsType = GetArchetypeChunkComponentType<TweenBounds>(true),
                ProgressType = GetArchetypeChunkComponentType<TweenProgress>(true),
                YoyoDirectionType = GetArchetypeChunkComponentType<TweenYoyoDirection>(true)
            }.Schedule(_tweens, inputDeps);
        }
    }
}