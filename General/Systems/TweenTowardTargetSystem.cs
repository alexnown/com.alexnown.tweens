using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class TweenTowardTargetSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveTowardTargetValueJob : IJobProcessComponentData<FloatContainer, TweenComplitedState, TweenSpeed, TweenFloatTarget>
        {
            public float Dt;

            public void Execute(ref FloatContainer result, ref TweenComplitedState complited,
                [ReadOnly]ref TweenSpeed speed, [ReadOnly] ref TweenFloatTarget targetValue)
            {
                var delta = speed.Value * Dt;
                var currValue = result.Value;
                float target = targetValue.Value;
                if (currValue < target)
                {
                    currValue += delta;
                    if (currValue > target)
                    {
                        currValue = target;
                        complited.IsComplited = true;
                    }
                }
                else if (currValue > target)
                {
                    currValue -= delta;
                    if (currValue < target)
                    {
                        currValue = target;
                        complited.IsComplited = true;
                    }
                }
                result.Value = currValue;
            }
        }

        struct RemoveTweenComponentsIfEnded : IJob
        {
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public EntityCommandBuffer Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntitieType;
            [ReadOnly]
            public ArchetypeChunkComponentType<DestroyOnComplite> DestroyType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenComplitedState> ComplitedType;

            public void Execute()
            {
                for (int chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    ArchetypeChunk chunk = Chunks[chunkIndex];
                    var entities = chunk.GetNativeArray(EntitieType);
                    var states = chunk.GetNativeArray(ComplitedType);
                    bool hasDestroy = chunk.Has(DestroyType);
                    for (int i = 0; i < entities.Length; i++)
                    {
                        if (states[i].IsComplited)
                        {
                            var entitie = entities[i];
                            if (hasDestroy)
                            {
                                Cb.DestroyEntity(entitie);
                            }
                            else
                            {
                                Cb.RemoveComponent<TweenSpeed>(entitie);
                                Cb.RemoveComponent<TweenComplitedState>(entitie);
                                Cb.RemoveComponent<TweenFloatTarget>(entitie);
                            }
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
                ComponentType.ReadOnly<TweenComplitedState>(),
                ComponentType.ReadOnly<TweenSpeed>(),
                ComponentType.ReadOnly<TweenFloatTarget>(),
                ComponentType.ReadOnly<FloatContainer>());
            RequireForUpdate(_tweens);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return inputDeps;
            var updateValueJob = new MoveTowardTargetValueJob { Dt = dt }.Schedule(this, inputDeps);
            return updateValueJob;
            /*
            var checkTweenEndedJob = new RemoveTweenComponentsIfEnded
            {
                Chunks = _tweens.CreateArchetypeChunkArray(Allocator.TempJob),
                Cb = _endBarrier.CreateCommandBuffer(),
                EntitieType = GetArchetypeChunkEntityType(),
                DestroyType = GetArchetypeChunkComponentType<DestroyOnComplite>(true),
                ComplitedType = GetArchetypeChunkComponentType<TweenComplitedState>(true)
            }.Schedule(updateValueJob);
            _endBarrier.AddJobHandleForProducer(checkTweenEndedJob); 
            return checkTweenEndedJob;*/
        }
    }
}
