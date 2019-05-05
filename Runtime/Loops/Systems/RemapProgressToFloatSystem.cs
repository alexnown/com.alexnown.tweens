using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    [UpdateAfter(typeof(UpdateLoopProgressSystem))]
    public class RemapProgressToFloatSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ProgressToFloatValueJob : IJobChunk
        {
            public ArchetypeChunkComponentType<FloatContainer> ValueType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;
            [ReadOnly]
            public ArchetypeChunkComponentType<RemapLoopProgressToFloat> RepamDataType;
            [ReadOnly]
            public ArchetypeChunkComponentType<ComplitedLoops> LoopsType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var values = chunk.GetNativeArray(ValueType);
                var tweenProgresses = chunk.GetNativeArray(ProgressType);
                var remapDataArray = chunk.GetNativeArray(RepamDataType);
                var loopsCounts = chunk.GetNativeArray(LoopsType);

                for (int i = 0; i < tweenProgresses.Length; i++)
                {
                    float progress = tweenProgresses[i].Value;
                    var remapData = remapDataArray[i];
                    if (remapData.IsYoyoTweening)
                    {
                        if (loopsCounts[i].Value % 2 == 1) progress = 1 - progress;
                    }
                    if (remapData.Easing != 0)
                    {
                        progress = EasingValue(progress, remapData.Easing);
                    }
                    var borders = remapData.ValueBorders;
                    progress = borders.x + progress * (borders.y - borders.x);
                    values[i] = new FloatContainer { Value = progress };
                }
            }


            private float EasingValue(float progress, byte type)
            {
                switch (type)
                {
                    case (byte)EasingType.OutQuad:
                        return -progress * (progress - 2);
                    case (byte)EasingType.InQuad:
                        return progress * progress;
                    case (byte)EasingType.InOutQuad:
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
        protected override void OnCreate()
        {
            _tweens = GetEntityQuery(ComponentType.ReadOnly<TweenProgress>(),
                ComponentType.ReadOnly<FloatContainer>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ProgressToFloatValueJob
            {
                ValueType = GetArchetypeChunkComponentType<FloatContainer>(),
                ProgressType = GetArchetypeChunkComponentType<TweenProgress>(true),
                LoopsType = GetArchetypeChunkComponentType<ComplitedLoops>(true),
                RepamDataType = GetArchetypeChunkComponentType<RemapLoopProgressToFloat>(true)
            }.Schedule(_tweens, inputDeps);
        }
    }
}