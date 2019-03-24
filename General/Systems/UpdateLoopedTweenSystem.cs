using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class UpdateLoopedTweenSystem : JobComponentSystem
    {
        struct LoopedTweenJob : IJobChunk
        {
            public float Dt;
            [ReadOnly]
            public EntityCommandBuffer Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<DestroyOnComplite> DestroyType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenBounds> BoundsType;
            public ArchetypeChunkComponentType<FloatContainer> FloatType;
            public ArchetypeChunkComponentType<TweenSpeed> SpeedType;
            public ArchetypeChunkComponentType<TweenLoops> LoopsType;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var valuesArray = chunk.GetNativeArray(FloatType);
                var speeds = chunk.GetNativeArray(SpeedType);
                var remainingLoops = chunk.GetNativeArray(LoopsType);
                var boundsArray = chunk.GetNativeArray(BoundsType);

                for (int i = 0; i < entities.Length; i++)
                {
                    float tweenSpeed = speeds[i].Value;
                    float delta = tweenSpeed * Dt;
                    var bounds = boundsArray[i];
                    float newAlphaValue = valuesArray[i].Value + delta;
                    bool reachBound = ReachBound(bounds.Min, bounds.Max, delta >= 0, ref newAlphaValue);

                    if (reachBound)
                    {
                        uint loops = remainingLoops.IsCreated ? remainingLoops[i].Value : 0;
                        if (loops > 1)
                        {
                            remainingLoops[i] = new TweenLoops { Value = loops - 1 };
                            speeds[i] = new TweenSpeed { Value = -tweenSpeed };
                        }
                        else
                        {
                            newAlphaValue = delta > 0 ? bounds.Max : bounds.Min;
                            var entitie = entities[i];
                            if (chunk.HasChunkComponent(DestroyType))
                            {
                                Cb.DestroyEntity(entitie);
                            }
                            else
                            {
                                Cb.RemoveComponent<TweenSpeed>(entitie);
                                Cb.RemoveComponent<TweenBounds>(entitie);
                                if (remainingLoops.IsCreated) Cb.RemoveComponent<TweenLoops>(entitie);
                            }
                        }
                    }
                    valuesArray[i] = new FloatContainer { Value = newAlphaValue };
                }
            }

            private bool ReachBound(float min, float max, bool moveForward, ref float value)
            {
                if (moveForward)
                {
                    if (value > max)
                    {
                        value = 2 * max - value;
                        return true;
                    }
                }
                else if (value < min)
                {
                    value = 2 * min - value;
                    return true;
                }
                return false;
            }
        }

        private ComponentGroup _tweens;
        private EndSimulationEntityCommandBufferSystem _endBarrier;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _tweens = GetComponentGroup(
                ComponentType.ReadWrite<FloatContainer>(),
                ComponentType.ReadOnly<TweenSpeed>(),
                ComponentType.ReadOnly<TweenBounds>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return inputDeps;
            var job = new LoopedTweenJob
            {
                Cb = _endBarrier.CreateCommandBuffer(),
                Dt = dt,
                EntityType = GetArchetypeChunkEntityType(),
                SpeedType = GetArchetypeChunkComponentType<TweenSpeed>(),
                DestroyType = GetArchetypeChunkComponentType<DestroyOnComplite>(true),
                BoundsType = GetArchetypeChunkComponentType<TweenBounds>(true),
                LoopsType = GetArchetypeChunkComponentType<TweenLoops>(),
                FloatType = GetArchetypeChunkComponentType<FloatContainer>()
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
