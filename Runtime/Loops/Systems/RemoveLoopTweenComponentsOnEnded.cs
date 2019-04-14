using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    /// <summary>
    /// Checks tween completing state and if it true, removes tween components, specific for looping tweens: progress, loops count, easing, remap bounds, loop time etc.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class RemoveLoopTweenComponentsOnEnded : JobComponentSystem
    {
        struct RemoveLoopTweenComponentsIfEnded : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkEntityType EntitieType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenComplitedState> ComplitedType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenBounds> BoundsType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenEasing> EasingType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenYoyoDirection> DirectionType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenLoops> LoopsType;
            [ReadOnly]
            public ArchetypeChunkBufferType<TweenTargetElement> ElementsBufferType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntitieType);
                var states = chunk.GetNativeArray(ComplitedType);
                bool hasBounds = chunk.Has(BoundsType);
                bool hasEasing = chunk.Has(EasingType);
                bool hasDirection = chunk.Has(DirectionType);
                bool hasLoops = chunk.Has(LoopsType);
                bool hasElementsBuffer = chunk.Has(ElementsBufferType);

                for (int i = 0; i < states.Length; i++)
                {
                    if (!states[i].IsComplited) continue;
                    var e = entities[i];
                    Cb.RemoveComponent<TweenComplitedState>(chunkIndex, e);
                    Cb.RemoveComponent<TweenLoopTime>(chunkIndex, e);
                    Cb.RemoveComponent<TweenProgress>(chunkIndex, e);
                    if (hasBounds) Cb.RemoveComponent<TweenBounds>(chunkIndex, e);
                    if (hasEasing) Cb.RemoveComponent<TweenEasing>(chunkIndex, e);
                    if (hasDirection) Cb.RemoveComponent<TweenYoyoDirection>(chunkIndex, e);
                    if (hasLoops) Cb.RemoveComponent<TweenLoops>(chunkIndex, e);
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
                All = new[] {
                    ComponentType.ReadOnly<TweenComplitedState>(),
                    ComponentType.ReadOnly<TweenLoopTime>(),
                    ComponentType.ReadOnly<TweenProgress>() },
                None = new[]
                {
                    ComponentType.ReadOnly<DestroyOnComplite>(),
                    ComponentType.ReadOnly<TweenSpeed>() ,
                    ComponentType.ReadOnly<TweenFloat3Speed>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var checkTweenEndedJob = new RemoveLoopTweenComponentsIfEnded
            {
                Cb = _endBarrier.CreateCommandBuffer().ToConcurrent(),
                EntitieType = GetArchetypeChunkEntityType(),
                ComplitedType = GetArchetypeChunkComponentType<TweenComplitedState>(true),
                EasingType = GetArchetypeChunkComponentType<TweenEasing>(true),
                DirectionType = GetArchetypeChunkComponentType<TweenYoyoDirection>(true),
                BoundsType = GetArchetypeChunkComponentType<TweenBounds>(true),
                LoopsType = GetArchetypeChunkComponentType<TweenLoops>(true),
                ElementsBufferType = GetArchetypeChunkBufferType<TweenTargetElement>(true)
            }.Schedule(_tweens, inputDeps);
            _endBarrier.AddJobHandleForProducer(checkTweenEndedJob);
            return checkTweenEndedJob;
        }
    }
}
