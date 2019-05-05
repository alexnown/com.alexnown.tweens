using Unity.Entities;

namespace EcsTweens
{
    public class TweenSystemGroup : ComponentSystemGroup
    {
        public override void SortSystemUpdateList()
        {
            SyncTweenResultToTargetEntitiesSystem syncSystem = null;
            foreach (var system in m_systemsToUpdate)
            {
                if (system is SyncTweenResultToTargetEntitiesSystem)
                {
                    syncSystem = system as SyncTweenResultToTargetEntitiesSystem;
                }
            }
            if(syncSystem == null) return;
            m_systemsToUpdate.Remove(syncSystem);
            base.SortSystemUpdateList();
            m_systemsToUpdate.Add(syncSystem);
        }
    }
}
