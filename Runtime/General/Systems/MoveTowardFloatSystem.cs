using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EcsTweens
{
    [UpdateInGroup(typeof(TweenSystemGroup))]
    public class MoveTowardFloatSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveTowardFloatJob : IJobForEach<FloatContainer, TweenCompliteState, TweenSpeed, TweenFloatTarget>
        {
            public float Dt;

            public void Execute(ref FloatContainer current, ref TweenCompliteState endState,
                [ReadOnly]ref TweenSpeed speed, [ReadOnly] ref TweenFloatTarget targetValue)
            {
                var delta = speed.Value * Dt;
                var currValue = current.Value;
                float target = targetValue.Value;
                if (currValue < target)
                {
                    currValue += delta;
                    if (currValue > target)
                    {
                        currValue = target;
                        endState.IsComplited = true;
                    }
                }
                else if (currValue > target)
                {
                    currValue -= delta;
                    if (currValue < target)
                    {
                        currValue = target;
                        endState.IsComplited = true;
                    }
                }
                current.Value = currValue;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MoveTowardFloatJob { Dt = Time.deltaTime }.Schedule(this, inputDeps);
        }
    }
}
