using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace EcsTweens
{
    /// <summary>
    /// Addition parameters to initialize loop tween. 
    /// </summary>
    [Serializable]
    public struct LoopTweenParams
    {
        public float ValueAtStart;
        public float ValueAtEnd;
        public int MaxLoops;
        public bool IsYoyoTweening;
        public EasingType Easing;
        public float StartTweenProgress;
    }

    public static class TweenBuilder
    {
        #region Create tweening entity

        public static Entity CreateTowardFloatTween(float start, float target, float speed,
            bool destroyOnComplite = false)
        {
            var archetypes = TweenArchetypes.Active;
            var archetype = archetypes.FloatTweenToward(destroyOnComplite);
            var entity = archetypes.TargetEntityManager.CreateEntity(archetype);
            SetTweenTowardFloat(archetypes.TargetEntityManager, entity, start, target, speed);
            return entity;
        }

        public static Entity CreateTowardFloat3Tween(float3 start, float3 target, float speed,
            bool destroyOnComplite = false)
        {
            var archetypes = TweenArchetypes.Active;
            var archetype = archetypes.Float3TweenToward(destroyOnComplite);
            var entity = archetypes.TargetEntityManager.CreateEntity(archetype);
            SetTweenTowardFloat3(archetypes.TargetEntityManager, entity, start, target, speed);
            return entity;
        }

        public static Entity CreateLoopTween(float loopTime, int maxLoops, bool isYoyoTweening = true,
            EasingType easing = EasingType.Linear)
        {
            return CreateLoopTween(loopTime, new LoopTweenParams
            {
                IsYoyoTweening = isYoyoTweening,
                MaxLoops = maxLoops,
                ValueAtEnd = 1,
                Easing = easing
            });
        }

        public static Entity CreateEndlessLoopTween(float loopTime, bool isYoyoTweening = true,
            EasingType easing = EasingType.Linear) => CreateLoopTween(loopTime, 0, isYoyoTweening, easing);

        public static Entity CreateLoopTween(float loopTime, LoopTweenParams tweenParams)
        {
            var archetypes = TweenArchetypes.Active;
            return CreateLoopTweenWithCustomArchetype(archetypes.TargetEntityManager,
                archetypes.LoopTweenArchetype(tweenParams.MaxLoops > 0),
                loopTime, tweenParams);
        }

        public static NativeArray<Entity> CreateLoopTweensWithCustomArchetype(EntityManager em, EntityArchetype tweenArchetype, int count,
            Allocator arrayAllocator, float loopTime, LoopTweenParams tweenParams)
        {
            var array = new NativeArray<Entity>(count, arrayAllocator);
            em.CreateEntity(tweenArchetype, array);
            for (int i = 0; i < array.Length; i++)
            {
                SetLoopTweenComponents(em, array[i], loopTime, tweenParams);
            }
            return array;
        }

        public static Entity CreateLoopTweenWithCustomArchetype(EntityManager em, EntityArchetype tweenArchetype, float loopTime, LoopTweenParams tweenParams)
        {
            var tweenEntity = em.CreateEntity(tweenArchetype);
            SetLoopTweenComponents(em, tweenEntity, loopTime, tweenParams);
            return tweenEntity;
        }

        #endregion

        #region Set tween components

        public static void SetTweenTowardFloat(EntityManager em, Entity e, float start, float target, float speed)
        {
            em.SetComponentData(e, new FloatContainer { Value = start });
            em.SetComponentData(e, new TweenFloatTarget { Value = target });
            em.SetComponentData(e, new TweenSpeed { Value = speed });
        }

        public static void SetTweenTowardFloat3(EntityManager em, Entity e, float3 start, float3 target, float speed)
        {
            em.SetComponentData(e, new Float3Container { Value = start });
            em.SetComponentData(e, new TweenFloat3Target { Value = target });
            em.SetComponentData(e, new TweenSpeed { Value = speed });
        }

        public static void SetLoopTweenComponents(EntityManager em, Entity e, float loopTime, LoopTweenParams tweenParams)
        {
            em.SetComponentData(e, new TweenProgressSpeed { Value = 1 / loopTime });
            em.SetComponentData(e, new TweenProgress { Value = tweenParams.StartTweenProgress });
            var remapParams = new RemapLoopProgressToFloat
            {
                ValueBorders = new float2(tweenParams.ValueAtStart, tweenParams.ValueAtEnd),
                IsYoyoTweening = tweenParams.IsYoyoTweening,
                Easing = (byte)tweenParams.Easing
            };
            em.SetComponentData(e, remapParams);
            if (tweenParams.MaxLoops > 0) em.SetComponentData(e, new MaxTweenLoops { Value = (ushort)tweenParams.MaxLoops });
        }
        #endregion

        #region Add toward tween components
        public static void InititalizeTowardDataTween(EntityManager em, Entity e, float target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(em, e, new TweenFloatTarget { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween(EntityManager em, Entity e, float3 target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(em, e, new TweenFloat3Target { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween<T>(EntityManager em, Entity e, T target, float speed,
            bool destroyOnComplite = false) where T : struct, IComponentData
        {
            em.AddComponentData(e, default(IsTweening));
            em.AddComponentData(e, default(TweenCompliteState));
            em.AddComponentData(e, target);
            em.AddComponentData(e, new TweenSpeed { Value = speed });
            if (destroyOnComplite) em.AddComponentData(e, default(DestroyOnComplite));
        }

        public static void InititalizeTowardDataTween(EntityCommandBuffer cb, Entity e, float target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(cb, e, new TweenFloatTarget { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween(EntityCommandBuffer cb, Entity e, float3 target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(cb, e, new TweenFloat3Target { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween<T>(EntityCommandBuffer cb, Entity e, T target, float speed,
            bool destroyOnComplite = false) where T : struct, IComponentData
        {
            cb.AddComponent(e, default(IsTweening));
            cb.AddComponent(e, default(TweenCompliteState));
            cb.AddComponent(e, target);
            cb.AddComponent(e, new TweenSpeed { Value = speed });
            if (destroyOnComplite) cb.AddComponent(e, default(DestroyOnComplite));
        }
        #endregion

        #region Add loop tween components
        public static void InititalizeLoopTween(EntityManager em, Entity e, float loopTime, int maxLoops, bool isYoyoTweening = true, EasingType easing = EasingType.Linear)
        {
            if (maxLoops <= 0) InititalizeEndlessLoopTween(em, e, loopTime, isYoyoTweening, easing);
            else InititalizeLoopTween(em, e, loopTime, new LoopTweenParams
            {
                MaxLoops = maxLoops,
                IsYoyoTweening = isYoyoTweening,
                ValueAtEnd = 1,
                Easing = easing
            });
        }

        public static void InititalizeEndlessLoopTween(EntityManager em, Entity e, float loopTime, bool isYoyoTweening = true, EasingType easing = EasingType.Linear)
        {
            InititalizeLoopTween(em, e, loopTime, new LoopTweenParams
            {
                IsYoyoTweening = isYoyoTweening,
                ValueAtEnd = 1,
                Easing = easing
            });
        }

        public static void InititalizeLoopTween(EntityManager em, Entity e, float loopTime, LoopTweenParams tweenParams)
        {
            em.AddComponentData(e, default(IsTweening));
            em.AddComponentData(e, new TweenProgressSpeed { Value = 1 / loopTime });
            em.AddComponentData(e, new TweenProgress { Value = tweenParams.StartTweenProgress });
            var remapParams = new RemapLoopProgressToFloat
            {
                ValueBorders = new float2(tweenParams.ValueAtStart, tweenParams.ValueAtEnd),
                IsYoyoTweening = tweenParams.IsYoyoTweening,
                Easing = (byte)tweenParams.Easing
            };
            em.AddComponentData(e, remapParams);
            if (tweenParams.MaxLoops > 0) em.AddComponentData(e, new MaxTweenLoops { Value = (ushort)tweenParams.MaxLoops });
        }

        public static void InititalizeLoopTween(EntityCommandBuffer cb, Entity e, float loopTime, int maxLoops, bool isYoyoTweening = true, EasingType easing = EasingType.Linear)
        {
            InititalizeLoopTween(cb, e, loopTime, new LoopTweenParams
            {
                MaxLoops = maxLoops,
                IsYoyoTweening = isYoyoTweening,
                ValueAtEnd = 1,
                Easing = easing
            });
        }

        public static void InititalizeEndlessLoopTween(EntityCommandBuffer cb, Entity e, float loopTime,
            bool isYoyoTweening = true, EasingType easing = EasingType.Linear)
            => InititalizeLoopTween(cb, e, loopTime, 0, isYoyoTweening, easing);

        public static void InititalizeLoopTween(EntityCommandBuffer cb, Entity e, float loopTime, LoopTweenParams tweenParams)
        {
            cb.AddComponent(e, default(IsTweening));
            cb.AddComponent(e, new TweenProgressSpeed { Value = 1 / loopTime });
            cb.AddComponent(e, new TweenProgress { Value = tweenParams.StartTweenProgress });
            var remapParams = new RemapLoopProgressToFloat
            {
                ValueBorders = new float2(tweenParams.ValueAtStart, tweenParams.ValueAtEnd),
                IsYoyoTweening = tweenParams.IsYoyoTweening,
                Easing = (byte)tweenParams.Easing
            };
            cb.AddComponent(e, remapParams);
            if (tweenParams.MaxLoops > 0) cb.AddComponent(e, new MaxTweenLoops { Value = (ushort)tweenParams.MaxLoops });
        }
        #endregion
    }
}
