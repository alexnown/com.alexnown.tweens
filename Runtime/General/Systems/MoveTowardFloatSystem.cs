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
        struct MoveTowardFloatJob : IJobProcessComponentData<FloatContainer, TweenComplitedState, TweenSpeed, TweenFloatTarget>
        {
            public float Dt;

            public void Execute(ref FloatContainer current, ref TweenComplitedState complited,
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
                        complited.IsComplited = true;
                    }
                }
                else if (currValue > target)
                {
                    currValue -= delta;
                    if (currValue < target)
                    {
                        currValue = target;
                        complited.IsComplited = true;
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
