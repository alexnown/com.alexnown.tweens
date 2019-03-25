using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    /// <summary>
    /// Share FloatValue from entity to all existing in TweenTargetElement buffer entities, which have FloatValue component.
    /// </summary>
    [UpdateInGroup(typeof(TweenSystemGroup))]
    [UpdateAfter(typeof(RemapProgressToFloatValue))]
    public class SyncTweenResultToTargetEntitiesSystem : JobComponentSystem
    {
        struct SyncFloatValueToTargets : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ComponentDataFromEntity<FloatContainer> TargetEntities;
            public ArchetypeChunkBufferType<TweenTargetElement> TargetsBufferType;
            [ReadOnly]
            public ArchetypeChunkComponentType<FloatContainer> ValueType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var progress = chunk.GetNativeArray(ValueType);
                var buffers = chunk.GetBufferAccessor(TargetsBufferType);
                for (int i = 0; i < buffers.Length; i++)
                {
                    var entitiesWithFloatContainer = buffers[i];
                    for (int j = 0; j < entitiesWithFloatContainer.Length; j++)
                    {
                        Entity targetEntity = entitiesWithFloatContainer[j].Entity;
                        bool targetEntityHasComponent = TargetEntities.Exists(targetEntity);
                        if (targetEntityHasComponent)
                        {
                            Cb.SetComponent(chunkIndex, targetEntity, new FloatContainer { Value = progress[i].Value });
                        }
                        else
                        {
                            entitiesWithFloatContainer.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
        }
        
        private TweensSyncValueBarrier _barrier;
        private ComponentGroup _syncFloatContainers;
        [Inject]
        private ComponentDataFromEntity<FloatContainer> _targetEntities;

        protected override void OnCreateManager()
        {
            _barrier = World.GetOrCreateManager<TweensSyncValueBarrier>();
            _syncFloatContainers = GetComponentGroup(ComponentType.ReadOnly<FloatContainer>(),
                ComponentType.ReadOnly<TweenTargetElement>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new SyncFloatValueToTargets
            {
                Cb = _barrier.CreateCommandBuffer().ToConcurrent(),
                TargetEntities = _targetEntities,
                ValueType = GetArchetypeChunkComponentType<FloatContainer>(),
                TargetsBufferType = GetArchetypeChunkBufferType<TweenTargetElement>()
            }.Schedule(_syncFloatContainers, inputDeps);
            _barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
