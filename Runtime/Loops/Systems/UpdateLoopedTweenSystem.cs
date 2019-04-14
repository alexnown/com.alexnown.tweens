using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    /// <summary>
    /// Updates tween progress, remaining loops count and completing tween state.
    /// </summary>
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class UpdateLoopedTweenSystem : JobComponentSystem
    {
        [BurstCompile]
        struct LoopedTweenJob : IJobChunk
        {
            public float Dt;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenLoopTime> TimeType;
            public ArchetypeChunkComponentType<TweenComplitedState> StateType;
            public ArchetypeChunkComponentType<TweenLoops> LoopsType;
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;
            public ArchetypeChunkComponentType<TweenYoyoDirection> YoyoDirectionType;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var valuesArray = chunk.GetNativeArray(ProgressType);
                var times = chunk.GetNativeArray(TimeType);
                var remainingLoops = chunk.GetNativeArray(LoopsType);
                var yoyoDirections = chunk.GetNativeArray(YoyoDirectionType);
                var state = chunk.GetNativeArray(StateType);
                for (int i = 0; i < entities.Length; i++)
                {
                    float delta = Dt / math.max(0.001f, times[i].Value);
                    float newProgress = valuesArray[i].Value + delta;

                    if (newProgress > 1)
                    {
                        bool isTweenEnded = false;
                        if (remainingLoops.IsCreated)
                        {
                            uint loops = remainingLoops[i].Value;
                            if (loops > 1) remainingLoops[i] = new TweenLoops { Value = loops - 1 };
                            else isTweenEnded = true;
                        }
                        if (isTweenEnded)
                        {
                            newProgress = 1;
                            state[i] = new TweenComplitedState { IsComplited = true };
                        }
                        else
                        {
                            newProgress = math.frac(newProgress);
                            if (yoyoDirections.IsCreated)
                            {
                                bool prevYoyoDir = yoyoDirections[i].IsBack;
                                yoyoDirections[i] = new TweenYoyoDirection { IsBack = !prevYoyoDir };
                            }
                        }
                    }
                    valuesArray[i] = new TweenProgress { Value = newProgress };
                }
            }
        }

        private EntityQuery _tweens;

        protected override void OnCreate()
        {
            _tweens = GetEntityQuery(
                ComponentType.ReadWrite<TweenProgress>(),
                ComponentType.ReadOnly<TweenLoopTime>(),
                ComponentType.ReadOnly<TweenComplitedState>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new LoopedTweenJob
            {
                Dt = Time.deltaTime,
                EntityType = GetArchetypeChunkEntityType(),
                TimeType = GetArchetypeChunkComponentType<TweenLoopTime>(true),
                StateType = GetArchetypeChunkComponentType<TweenComplitedState>(),
                LoopsType = GetArchetypeChunkComponentType<TweenLoops>(),
                ProgressType = GetArchetypeChunkComponentType<TweenProgress>(),
                YoyoDirectionType = GetArchetypeChunkComponentType<TweenYoyoDirection>()
            }.Schedule(_tweens, inputDeps);
            return job;
        }
    }
}
