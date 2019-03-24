using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class TweenTowardTargetSystem : JobComponentSystem
    {
        struct MoveTowardTweenJob : IJobChunk
        {
            public float Dt;
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntitieType;
            [ReadOnly]
            public ArchetypeChunkComponentType<DestroyOnComplite> DestroyType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenSpeed> SpeedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenFloatTarget> TargetType;
            public ArchetypeChunkComponentType<FloatContainer> FloatType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntitieType);
                var values = chunk.GetNativeArray(FloatType);
                var targets = chunk.GetNativeArray(TargetType);
                var speeds = chunk.GetNativeArray(SpeedType);
                bool hasDestroy = chunk.HasChunkComponent(DestroyType);

                for (int i = 0; i < entities.Length; i++)
                {
                    float currValue = values[i].Value;
                    float target = targets[i].Value;
                    var delta = speeds[i].Value * Dt;
                    bool complited;
                    if (currValue < target)
                    {
                        currValue += delta;
                        complited = currValue >= target;
                    }
                    else if (currValue > target)
                    {
                        currValue -= delta;
                        complited = currValue <= target;
                    }
                    else complited = true;

                    values[i] = new FloatContainer { Value = currValue };
                    if (complited)
                    {
                        var entitie = entities[i];
                        if (hasDestroy)
                        {
                            Cb.DestroyEntity(chunkIndex, entitie);
                        }
                        else
                        {
                            Cb.RemoveComponent<TweenSpeed>(chunkIndex, entitie);
                            Cb.RemoveComponent<TweenFloatTarget>(chunkIndex, entitie);
                        }
                    }
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endBarrier;
        private ComponentGroup _tweens;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _tweens = GetComponentGroup(
                ComponentType.ReadOnly<TweenSpeed>(), 
                ComponentType.ReadOnly<TweenFloatTarget>(),
                ComponentType.ReadWrite<FloatContainer>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return inputDeps;
            var job = new MoveTowardTweenJob
            {
                Cb = _endBarrier.CreateCommandBuffer().ToConcurrent(),
                Dt = dt,
                EntitieType = GetArchetypeChunkEntityType(),
                SpeedType = GetArchetypeChunkComponentType<TweenSpeed>(true),
                TargetType = GetArchetypeChunkComponentType<TweenFloatTarget>(true),
                DestroyType = GetArchetypeChunkComponentType<DestroyOnComplite>(true),
                FloatType = GetArchetypeChunkComponentType<FloatContainer>()
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
