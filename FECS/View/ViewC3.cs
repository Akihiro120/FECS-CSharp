using FECS.Manager;
using FECS.Containers;
using FECS.Core;

namespace FECS.View
{
    public static class ViewHolder<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        public static View<T1, T2, T3> ViewInstance = new View<T1, T2, T3>();
    }

    public class View<T1, T2, T3> : ViewBase
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        public delegate void EachDelegate(Entity entity, ref T1 component1, ref T2 component2, ref T3 component3);

        private (SparseSet<T1> p1, SparseSet<T2> p2, SparseSet<T3> p3) m_Pools;

        public View() : base(3)
        {
        }

        public View<T1, T2, T3> With<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        public View<T1, T2, T3> Without<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => !ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>() ||
                   m_Versions[1] != ComponentManager.GetVersion<T2>() ||
                   m_Versions[2] != ComponentManager.GetVersion<T3>();
        }

        protected override void InitializePools()
        {
            m_Pools.p1 = ComponentManager.GetPool<T1>(m_EntityManager);
            m_Pools.p2 = ComponentManager.GetPool<T2>(m_EntityManager);
            m_Pools.p3 = ComponentManager.GetPool<T3>(m_EntityManager);
        }

        protected override void PopulateCache()
        {
            List<ISparseSet> pools = new List<ISparseSet> { m_Pools.p1, m_Pools.p2, m_Pools.p3 };
            ISparseSet driverPool = pools.OrderBy(p => p.Size()).First();

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);
                if (m_Pools.p1.Has(entity) && m_Pools.p2.Has(entity) && m_Pools.p3.Has(entity))
                {
                    m_Cache.Add(entity);
                }
            }
        }

        protected override void UpdateLastVersions()
        {
            m_Versions[0] = ComponentManager.GetVersion<T1>();
            m_Versions[1] = ComponentManager.GetVersion<T2>();
            m_Versions[2] = ComponentManager.GetVersion<T3>();
        }

        public void Each(EachDelegate fn)
        {
            CheckAndRebuildCache();

            foreach (Entity entity in m_Cache)
            {
                if (m_FilterPredicates.Count > 0)
                {
                    if (PassesAllFilters(entity))
                    {
                        fn(entity,
                           ref m_Pools.p1.Get(entity),
                           ref m_Pools.p2.Get(entity),
                           ref m_Pools.p3.Get(entity));
                    }
                }
                else
                {
                    fn(entity,
                       ref m_Pools.p1.Get(entity),
                       ref m_Pools.p2.Get(entity),
                       ref m_Pools.p3.Get(entity));
                }
            }

            m_FilterPredicates.Clear();
        }
    }
}

