using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class UpdateEndlessTweenSystem : JobComponentSystem
    {
        [BurstCompile]
        [ExcludeComponent(typeof(TweenFloatTarget))]
        struct EndlessTweenJob : IJobProcessComponentData<FloatContainer, TweenSpeed>
        {
            public float Dt;
            public void Execute(ref FloatContainer value, [ReadOnly] ref TweenSpeed speed)
            {
                value.Value += speed.Value * Dt;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return inputDeps;
            return new EndlessTweenJob { Dt = dt }.Schedule(this, inputDeps);
        }
    }
}
