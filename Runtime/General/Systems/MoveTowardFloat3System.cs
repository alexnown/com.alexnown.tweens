using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class MoveTowardFloat3System : JobComponentSystem
    {
        [BurstCompile]
        struct MoveTowardFloat3Job : IJobForEach<Float3Container, TweenComplitedState, TweenSpeed, TweenFloat3Target>
        {
            public float Dt;

            public void Execute(ref Float3Container current, ref TweenComplitedState state,
                [ReadOnly]ref TweenSpeed speed, [ReadOnly]ref TweenFloat3Target target)
            {
                var offset = target.Value - current.Value;
                var magnitude = math.length(offset);
                var maxDelta = speed.Value * Dt;
                if (magnitude <= maxDelta || magnitude == 0)
                {
                    state.IsComplited = true;
                    current.Value = target.Value;
                }
                else current.Value += offset / magnitude * maxDelta;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MoveTowardFloat3Job { Dt = Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}
