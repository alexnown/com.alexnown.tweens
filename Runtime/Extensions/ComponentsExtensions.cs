using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace EcsTweens
{
    public interface ITweenedComponent { }

    public static class ComponentsExtensions
    {
        #region Loop tweens
        public static Entity AlphaYoyoTweening(this SpriteRenderer rend, float loopTime, float startAlpha, float endAlpha, int maxLoops = 0)
            => CreateTweeningLoop(rend, loopTime, new LoopTweenParams { ValueAtStart = startAlpha, ValueAtEnd = endAlpha, MaxLoops = maxLoops, IsYoyoTweening = true });

        public static Entity AlphaTweeningLoop(this SpriteRenderer rend, float loopTime, LoopTweenParams tweenParams)
            => CreateTweeningLoop(rend, loopTime, tweenParams);

        public static Entity YoyoTweeningLoop<T>(this T component, float loopTime, float startValue, float endValue, int maxLoops = 0)
            where T : Component, ITweenedComponent
            => CreateTweeningLoop(component, loopTime, new LoopTweenParams { ValueAtStart = startValue, ValueAtEnd = endValue, MaxLoops = maxLoops, IsYoyoTweening = true });

        public static Entity TweeningLoop<T>(this T component, float loopTime, LoopTweenParams tweenParams)
            where T : Component, ITweenedComponent => CreateTweeningLoop(component, loopTime, tweenParams);

        private static Entity CreateTweeningLoop(Component component, float loopTime, LoopTweenParams tweenParams)
        {
            var tween = TweenBuilder.CreateLoopTween(loopTime, tweenParams);
            World.Active.EntityManager.AddComponentObject(tween, component);
            return tween;
        }
        #endregion


        #region Toward tweens
        public static Entity AlphaTweening(this SpriteRenderer rend, float startValue, float endValue,
            float tweenTime, bool destroyOnComplite = true)
            => InitializeComponentTweening(rend, startValue, endValue, tweenTime, destroyOnComplite);

        public static Entity TweeningToward<T>(this T component, float startValue, float endValue,
            float tweenTime, bool destroyOnComplite = true) where T : Component, ITweenedComponent
            => InitializeComponentTweening(component, startValue, endValue, tweenTime, destroyOnComplite);

        private static Entity InitializeComponentTweening(Component component, float startValue, float endValue, float tweenTime, bool destroyOnComplite = true)
        {
            var tweenEntity = TweenBuilder.CreateTowardFloatTween(startValue, endValue, math.abs(endValue - startValue) / tweenTime, destroyOnComplite);
            World.Active.EntityManager.AddComponentObject(tweenEntity, component);
            return tweenEntity;
        }
        #endregion



    }
}
