using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EcsTweens
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class DestroyEndedTweenSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(DestroyOnComplite))]
        [BurstCompile]
        struct DestroyComplitedTweenEntities : IJobProcessComponentDataWithEntity<TweenComplitedState>
        {
            public EntityCommandBuffer Cb;
            public void Execute(Entity entity, int index, [ReadOnly] ref TweenComplitedState state)
            {
                if (state.IsComplited) Cb.DestroyEntity(entity);
            }
        }
        
        private EndSimulationEntityCommandBufferSystem _endBarrier;

        protected override void OnCreateManager()
        {
            _endBarrier = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new DestroyComplitedTweenEntities
            {
                Cb = _endBarrier.CreateCommandBuffer()
            }.ScheduleSingle(this, inputDeps);
            _endBarrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
