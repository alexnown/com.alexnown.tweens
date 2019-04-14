using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    /// <summary>
    /// Checks tween completing state and if it true, removes tween components, specific for "move toward" tweens: speed, target, state.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class RemoveTweenComponentsOnEndedSystem : JobComponentSystem
    {
        struct RemoveTweenComponentsIfEnded : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntitieType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenComplitedState> ComplitedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenFloatTarget> TargetType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenFloat3Target> Target3Type;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenSpeed> SpeedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenFloat3Speed> Speed3Type;
            [ReadOnly]
            public ArchetypeChunkBufferType<TweenTargetElement> ElementsBufferType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntitieType);
                var states = chunk.GetNativeArray(ComplitedType);
                bool hasTarget = chunk.Has(TargetType);
                bool hasTarget3 = chunk.Has(Target3Type);
                bool hasSpeed = chunk.Has(SpeedType);
                bool hasSpeed3 = chunk.Has(Speed3Type);
                bool hasElementsBuffer = chunk.Has(ElementsBufferType);

                for (int i = 0; i < states.Length; i++)
                {
                    if (!states[i].IsComplited) continue;
                    var e = entities[i];
                    Cb.RemoveComponent<TweenComplitedState>(chunkIndex, e);
                    if (hasTarget) Cb.RemoveComponent<TweenFloatTarget>(chunkIndex, e);
                    if (hasTarget3) Cb.RemoveComponent<TweenFloat3Target>(chunkIndex, e);
                    if (hasSpeed) Cb.RemoveComponent<TweenSpeed>(chunkIndex, e);
                    if (hasSpeed3) Cb.RemoveComponent<TweenFloat3Speed>(chunkIndex, e);
                    if (hasElementsBuffer) Cb.RemoveComponent<TweenTargetElement>(chunkIndex, e);
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endBarrier;
        private EntityQuery _tweens;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _tweens = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<TweenComplitedState>() },
                Any = new[]{
                    ComponentType.ReadOnly<TweenSpeed>() ,
                    ComponentType.ReadOnly<TweenFloat3Speed>()
                },
                None = new[] { ComponentType.ReadOnly<DestroyOnComplite>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var checkTweenEndedJob = new RemoveTweenComponentsIfEnded
            {
                Cb = _endBarrier.CreateCommandBuffer().ToConcurrent(),
                EntitieType = GetArchetypeChunkEntityType(),
                SpeedType = GetArchetypeChunkComponentType<TweenSpeed>(true),
                ComplitedType = GetArchetypeChunkComponentType<TweenComplitedState>(true),
                TargetType = GetArchetypeChunkComponentType<TweenFloatTarget>(true),
                Speed3Type = GetArchetypeChunkComponentType<TweenFloat3Speed>(true),
                Target3Type = GetArchetypeChunkComponentType<TweenFloat3Target>(true),
                ElementsBufferType = GetArchetypeChunkBufferType<TweenTargetElement>(true)
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(checkTweenEndedJob);
            return checkTweenEndedJob;
        }
    }
}
