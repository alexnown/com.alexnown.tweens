using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    [UpdateAfter(typeof(UpdateLoopedTweenSystem))]
    public class RemapProgressToFloatValue : JobComponentSystem
    {
        struct RemapTweenProgressToFloats : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkBufferType<HasFloatElement> ContainersBufferType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var progress = chunk.GetNativeArray(ProgressType);
                var buffers = chunk.GetBufferAccessor(ContainersBufferType);
                for (int i = 0; i < buffers.Length; i++)
                {
                    var entitiesWithFloatContainer = buffers[i];
                    for (int j = 0; j < entitiesWithFloatContainer.Length; j++)
                    {
                        Cb.SetComponent(chunkIndex, entitiesWithFloatContainer[j].Entity, new FloatContainer { Value = progress[i].Value });
                    }
                }
            }
        }

        struct RemapTweenProgressToFloat : IJobChunk
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent Cb;
            [ReadOnly]
            public ArchetypeChunkComponentType<LinkToEntity> LinkType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TweenProgress> ProgressType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var progress = chunk.GetNativeArray(ProgressType);
                var links = chunk.GetNativeArray(LinkType);
                for (int i = 0; i < links.Length; i++)
                {
                    Cb.SetComponent(chunkIndex, links[i].e, new FloatContainer { Value = progress[i].Value });
                }
            }
        }

        struct RemapProcess : IJobProcessComponentData<LinkToEntity, TweenProgress>
        {
            [ReadOnly]
            public EntityCommandBuffer Cb;

            public void Execute([ReadOnly]ref LinkToEntity link, [ReadOnly]ref TweenProgress progress)
            {
                Cb.SetComponent(link.e, new FloatContainer { Value = progress.Value });
            }
        }

        private EndSimulationEntityCommandBufferSystem _barrier;
        //private ComponentGroup _group;
        protected override void OnCreateManager()
        {
            _barrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new RemapProcess
            {
                Cb = _barrier.CreateCommandBuffer()
            }.ScheduleSingle(this, inputDeps);
            _barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}