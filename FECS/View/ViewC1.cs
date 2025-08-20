using FECS.Manager;
using FECS.Containers;

using FECS.Core;

namespace FECS.View
{
    public static class ViewHolder<T1>
        where T1 : struct
    {
        public static View<T1> ViewInstance = new View<T1>();
    }

    public class View<T1> : ViewBase
        where T1 : struct
    {
        public delegate void EachDelegate(Entity entity, ref T1 component1);
        private SparseSet<T1> m_Pools = null!;

        public View() : base(1)
        {

        }

        public View<T1> With<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        public View<T1> Without<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => !ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>();
        }

        protected override void InitializePools()
        {
            m_Pools = ComponentManager.GetPool<T1>(m_EntityManager);
        }

        protected override void PopulateCache()
        {
            ISparseSet driverPool = m_Pools;

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);
                if (m_Pools.Has(entity))
                {
                    m_Cache.Add(entity);
                }
            }
        }

        protected override void UpdateLastVersions()
        {
            m_Versions[0] = ComponentManager.GetVersion<T1>();
        }

        public void Each(EachDelegate fn)
        {
            CheckAndRebuildCache();

            foreach (Entity entity in m_Cache)
            {
                // NOTE: Solution to Filter Performance: 
                // we run the filters if available, then clear, otherwise we run the cache normally
                if (m_FilterPredicates.Count > 0)
                {
                    if (PassesAllFilters(entity))
                    {

                        fn(entity, ref m_Pools.Get(entity));
                    }
                }
                else
                {
                    fn(entity, ref m_Pools.Get(entity));
                }
            }

            m_FilterPredicates.Clear();
        }
    }
}
