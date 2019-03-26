using Unity.Entities;
using Unity.Mathematics;

namespace EcsTweens
{
    /// <summary>
    /// Addition parameters to initialize loop tween. 
    /// </summary>
    public struct LoopTweenParams
    {
        public float StartTweenProgress;
        public int LoopsRestriction;
        public EasingType Easing;
        public bool DestroyOnComplete;
    }

    public static class TweenBuilder
    {
        #region Toward tweens
        public static void InititalizeTowardDataTween(EntityManager em, Entity e, float target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(em, e, new TweenFloatTarget { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween(EntityManager em, Entity e, float3 target, float speed,
            bool destroyOnComplite = false)
            => InititalizeTowardDataTween(em, e, new TweenFloat3Target { Value = target }, speed, destroyOnComplite);

        public static void InititalizeTowardDataTween<T>(EntityManager em, Entity e, T target, float speed,
            bool destroyOnComplite = false) where T : struct, IComponentData
        {
            em.AddComponentData(e, default(TweenComplitedState));
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
            cb.AddComponent(e, default(TweenComplitedState));
            cb.AddComponent(e, target);
            cb.AddComponent(e, new TweenSpeed { Value = speed });
            if (destroyOnComplite) cb.AddComponent(e, default(DestroyOnComplite));
        }
        #endregion

        #region Loop tweens
        public static void InititalizeLoopTween(EntityManager em, Entity e, float loopTime,
            LoopTweenParams tweenParams = default(LoopTweenParams))
        {
            em.AddComponentData(e, default(TweenComplitedState));
            em.AddComponentData(e, new TweenLoopTime { Value = loopTime });
            em.AddComponentData(e, new TweenProgress { Value = tweenParams.StartTweenProgress });
            if (tweenParams.LoopsRestriction > 0) em.AddComponentData(e, new TweenLoops { Value = (uint)tweenParams.LoopsRestriction });
            if (tweenParams.DestroyOnComplete) em.AddComponentData(e, default(DestroyOnComplite));
            if (tweenParams.Easing != EasingType.Linear) em.AddComponentData(e, new TweenEasing(tweenParams.Easing));
        }
        
        public static void InititalizeLoopTween(EntityCommandBuffer cb, Entity e, float loopTime,
            LoopTweenParams tweenParams = default(LoopTweenParams))
        {
            cb.AddComponent(e, default(TweenComplitedState));
            cb.AddComponent(e, new TweenLoopTime { Value = loopTime });
            cb.AddComponent(e, new TweenProgress { Value = tweenParams.StartTweenProgress });
            if (tweenParams.LoopsRestriction > 0) cb.AddComponent(e, new TweenLoops { Value = (uint)tweenParams.LoopsRestriction });
            if (tweenParams.DestroyOnComplete) cb.AddComponent(e, default(DestroyOnComplite));
            if (tweenParams.Easing != EasingType.Linear) cb.AddComponent(e, new TweenEasing(tweenParams.Easing));
        }

        public static void InititalizeYoyoLoopTween(EntityManager em, Entity e, float loopTime,
            bool yoyoIsForward = true, LoopTweenParams tweenParams = default(LoopTweenParams))
        {
            InititalizeLoopTween(em, e, loopTime, tweenParams);
            em.AddComponentData(e, new TweenYoyoDirection { IsBack = !yoyoIsForward });
        }

        public static void InititalizeYoyoLoopTween(EntityManager em, Entity e, float loopTime, int loops = 2, bool yoyoIsForward = true)
            => InititalizeYoyoLoopTween(em, e, loopTime, yoyoIsForward, new LoopTweenParams { LoopsRestriction = loops });

        public static void InititalizeYoyoLoopTween(EntityCommandBuffer cb, Entity e, float loopTime,
            bool yoyoIsForward = true, LoopTweenParams tweenParams = default(LoopTweenParams))
        {
            InititalizeLoopTween(cb, e, loopTime, tweenParams);
            cb.AddComponent(e, new TweenYoyoDirection { IsBack = !yoyoIsForward });
        }

        public static void InititalizeYoyoLoopTween(EntityCommandBuffer cb, Entity e, float loopTime, int loops = 2, bool yoyoIsForward = true)
            => InititalizeYoyoLoopTween(cb, e, loopTime, yoyoIsForward, new LoopTweenParams { LoopsRestriction = loops });
        #endregion
    }
}
