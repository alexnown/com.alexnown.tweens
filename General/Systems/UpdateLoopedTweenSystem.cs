using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class UpdateLoopedTweenSystem : JobComponentSystem
    {
        [BurstCompile]
        struct LoopedTweenJob : IJobChunk
        {
            public float Dt;
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            //[ReadOnly]
            //public ArchetypeChunkComponentType<DestroyOnComplite> DestroyType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenLoopTime> TimeType;
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
                            if (loops > 1) remainingLoops[i] = new TweenLoops {Value = loops - 1};
                            else isTweenEnded = true;
                        }
                        if (isTweenEnded)
                        {
                            newProgress = 1;
                            var entitie = entities[i];
                            Cb.DestroyEntity(chunkIndex, entitie);
                            //if (chunk.HasChunkComponent(DestroyType))
                            //{
                            //    Cb.DestroyEntity(chunkIndex, entitie);
                            //}
                            //else
                            //{
                            //    Cb.RemoveComponent<TweenLoopTime>(chunkIndex, entitie);
                            //    Cb.RemoveComponent<TweenBounds>(chunkIndex, entitie);
                            //    if (remainingLoops.IsCreated) Cb.RemoveComponent<TweenLoops>(chunkIndex, entitie);
                            //}
                        }
                        else
                        {
                            newProgress = math.frac(newProgress);
                            if (yoyoDirections.IsCreated)
                            {
                                bool prevYoyoDir = yoyoDirections[i].IsForward;
                                yoyoDirections[i] = new TweenYoyoDirection { IsForward = !prevYoyoDir };
                            }
                        }
                    }
                    valuesArray[i] = new TweenProgress { Value = newProgress };
                }
            }
        }

        private ComponentGroup _tweens;
        private EndSimulationEntityCommandBufferSystem _endBarrier;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _tweens = GetComponentGroup(
                ComponentType.ReadWrite<TweenProgress>(),
                ComponentType.ReadOnly<TweenLoopTime>(),
                ComponentType.ReadOnly<TweenBounds>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return inputDeps;
            var job = new LoopedTweenJob
            {
                Cb = _endBarrier.CreateCommandBuffer().ToConcurrent(),
                Dt = dt,
                EntityType = GetArchetypeChunkEntityType(),
                //DestroyType = GetArchetypeChunkComponentType<DestroyOnComplite>(true),
                TimeType = GetArchetypeChunkComponentType<TweenLoopTime>(true),
                LoopsType = GetArchetypeChunkComponentType<TweenLoops>(),
                ProgressType = GetArchetypeChunkComponentType<TweenProgress>(),
                YoyoDirectionType = GetArchetypeChunkComponentType<TweenYoyoDirection>()
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
