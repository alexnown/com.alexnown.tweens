using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EcsTweens
{
    public class UpdateEndlessFloat3TweenSystem : JobComponentSystem
    {
        [BurstCompile]
        struct EndlessTweenJob : IJobProcessComponentData<Float3Container, TweenFloat3Speed>
        {
            public float Dt;
            public void Execute(ref Float3Container value, [ReadOnly]ref TweenFloat3Speed speed)
            {
                value.Value += speed.Value * Dt;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new EndlessTweenJob { Dt = Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}
