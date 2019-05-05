using Unity.Entities;

namespace EcsTweens
{
    public class TweenArchetypes
    {
        public readonly EntityManager TargetEntityManager;

        public static TweenArchetypes Active
        {
            get
            {
                if (_active == null)
                {
                    _active = new TweenArchetypes(World.Active.EntityManager);
                    _active.CreateArchetypes();
                }
                return _active;
            }
            set { _active = value; }
        }

        private static TweenArchetypes _active;
        private EntityArchetype _tweenFloatTowardWithDestroy;
        private EntityArchetype _tweenFloatTowardWithoutDestroy;
        private EntityArchetype _tweenFloat3TowardWithDestroy;
        private EntityArchetype _tweenFloat3TowardWithoutDestroy;

        private EntityArchetype _loopTween;
        private EntityArchetype _endlessLoopTween;


        public TweenArchetypes(EntityManager em)
        {
            TargetEntityManager = em;
        }

        private void CreateArchetypes()
        {
            #region Tween toward float
            _tweenFloatTowardWithDestroy = TargetEntityManager.CreateArchetype(
                   ComponentType.ReadOnly<IsTweening>(),
                   ComponentType.ReadOnly<FloatContainer>(),
                   ComponentType.ReadOnly<TweenFloatTarget>(),
                   ComponentType.ReadOnly<TweenSpeed>(),
                   ComponentType.ReadOnly<TweenCompliteState>(),
                   ComponentType.ReadOnly<DestroyOnComplite>());
            _tweenFloatTowardWithoutDestroy = TargetEntityManager.CreateArchetype(
                   ComponentType.ReadOnly<IsTweening>(),
                   ComponentType.ReadOnly<FloatContainer>(),
                   ComponentType.ReadOnly<TweenFloatTarget>(),
                   ComponentType.ReadOnly<TweenSpeed>(),
                   ComponentType.ReadOnly<TweenCompliteState>());
            #endregion

            #region Tween toward float3
            _tweenFloat3TowardWithDestroy = TargetEntityManager.CreateArchetype(
                   ComponentType.ReadOnly<IsTweening>(),
                   ComponentType.ReadOnly<Float3Container>(),
                   ComponentType.ReadOnly<TweenFloat3Target>(),
                   ComponentType.ReadOnly<TweenSpeed>(),
                   ComponentType.ReadOnly<TweenCompliteState>(),
                   ComponentType.ReadOnly<DestroyOnComplite>());
            _tweenFloat3TowardWithoutDestroy = TargetEntityManager.CreateArchetype(
                   ComponentType.ReadOnly<IsTweening>(),
                   ComponentType.ReadOnly<Float3Container>(),
                   ComponentType.ReadOnly<TweenFloat3Target>(),
                   ComponentType.ReadOnly<TweenSpeed>(),
                   ComponentType.ReadOnly<TweenCompliteState>());
            #endregion

            #region Loop tweens
            _endlessLoopTween = TargetEntityManager.CreateArchetype(
                ComponentType.ReadOnly<IsTweening>(),
                ComponentType.ReadOnly<ComplitedLoops>(),
                ComponentType.ReadOnly<TweenProgress>(),
                ComponentType.ReadOnly<TweenProgressSpeed>(),
                ComponentType.ReadOnly<RemapLoopProgressToFloat>(),
                ComponentType.ReadOnly<FloatContainer>());

            _loopTween = TargetEntityManager.CreateArchetype(
                ComponentType.ReadOnly<IsTweening>(),
                ComponentType.ReadOnly<ComplitedLoops>(),
                ComponentType.ReadOnly<TweenProgress>(),
                ComponentType.ReadOnly<TweenProgressSpeed>(),
                ComponentType.ReadOnly<RemapLoopProgressToFloat>(),
                ComponentType.ReadOnly<FloatContainer>(),
                ComponentType.ReadOnly<MaxTweenLoops>());
            #endregion
        }

        public EntityArchetype LoopTweenArchetype(bool loopsLimit)
        {
            return loopsLimit ? _loopTween : _endlessLoopTween;
        }

        public EntityArchetype FloatTweenToward(bool destroyOnComplite)
        {
            return destroyOnComplite ? _tweenFloatTowardWithDestroy : _tweenFloatTowardWithoutDestroy;
        }

        public EntityArchetype Float3TweenToward(bool destroyOnComplite)
        {
            return destroyOnComplite ? _tweenFloat3TowardWithDestroy : _tweenFloat3TowardWithoutDestroy;
        }
    }
}
