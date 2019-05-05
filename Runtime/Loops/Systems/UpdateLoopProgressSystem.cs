using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    /// <summary>
    /// Updates tween progress and ended loops count.
    /// </summary>
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class UpdateLoopProgressSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ProgressLoops : IJobForEachWithEntity<TweenProgress, TweenProgressSpeed, ComplitedLoops>
        {
            [ReadOnly]
            public EntityCommandBuffer DestroyCb;
            [ReadOnly]
            public ComponentDataFromEntity<MaxTweenLoops> MaxLoopsEntities;
            public float Dt;

            public void Execute(Entity entity, int index, ref TweenProgress progress, [ReadOnly]ref TweenProgressSpeed speed, ref ComplitedLoops loops)
            {
                float newProgress = progress.Value + speed.Value * Dt;
                if (newProgress >= 1)
                {
                    newProgress = math.frac(newProgress);
                    if (MaxLoopsEntities.Exists(entity))
                    {
                        var maxLoopsValue = MaxLoopsEntities[entity].Value;
                        if (loops.Value + 1 >= maxLoopsValue)
                        {
                            progress.Value = 1;
                            DestroyCb.DestroyEntity(entity);
                            return;
                        }
                    }
                    loops.Value++;
                }
                progress.Value = newProgress;
            }
        }

        private EndSimulationEntityCommandBufferSystem _destroyBarrier;
        protected override void OnCreate()
        {
            _destroyBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new ProgressLoops
            {
                Dt = Time.deltaTime,
                MaxLoopsEntities = GetComponentDataFromEntity<MaxTweenLoops>(),
                DestroyCb = _destroyBarrier.CreateCommandBuffer()
            }.ScheduleSingle(this, inputDeps);
            _destroyBarrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
