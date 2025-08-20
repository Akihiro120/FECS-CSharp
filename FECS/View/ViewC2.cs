using FECS.Manager;
using FECS.Containers;

using FECS.Core;

namespace FECS.View
{
    public static class ViewHolder<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        public static View<T1, T2> ViewInstance = new View<T1, T2>();
    }

    public class View<T1, T2> : ViewBase
        where T1 : struct
        where T2 : struct
    {
        public delegate void EachDelegate(Entity entity, ref T1 component1, ref T2 component2);
        private (SparseSet<T1> p1, SparseSet<T2> p2) m_Pools;

        public View() : base(2)
        {

        }

        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>() ||
                   m_Versions[1] != ComponentManager.GetVersion<T2>();
        }

        protected override void InitializePools()
        {
            m_Pools.p1 = ComponentManager.GetPool<T1>(m_EntityManager);
            m_Pools.p2 = ComponentManager.GetPool<T2>(m_EntityManager);
        }

        protected override void PopulateCache()
        {
            List<ISparseSet> pools = new List<ISparseSet> { m_Pools.p1, m_Pools.p2 };
            ISparseSet driverPool = pools.OrderBy(p => p.Size()).First();

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);
                if (m_Pools.p1.Has(entity) && m_Pools.p2.Has(entity))
                {
                    m_Cache.Add(entity);
                }
            }
        }

        protected override void UpdateLastVersions()
        {
            m_Versions[0] = ComponentManager.GetVersion<T1>();
            m_Versions[1] = ComponentManager.GetVersion<T2>();
        }

        public void Each(EachDelegate fn)
        {
            CheckAndRebuildCache();
            foreach (Entity entity in m_Cache)
            {
                fn(entity, ref m_Pools.p1.Get(entity), ref m_Pools.p2.Get(entity));
            }
        }
    }
}
