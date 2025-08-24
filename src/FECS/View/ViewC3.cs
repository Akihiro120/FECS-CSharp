using FECS.Manager;
using FECS.Containers;
using FECS.Core;

namespace FECS.View
{
    /// <summary>
    /// Provides a cached <see cref="View{T1, T2, T3}"/> singleton for reuse.
    /// Ensures only one instance exists per component-type triple.
    /// </summary>
    /// <typeparam name="T1">First component type.</typeparam>
    /// <typeparam name="T2">Second component type.</typeparam>
    /// <typeparam name="T3">Third component type.</typeparam>
    public static class ViewHolder<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        /// <summary>
        /// Shared singleton instance of <see cref="View{T1, T2, T3}"/>.
        /// </summary>
        public static View<T1, T2, T3> ViewInstance = new View<T1, T2, T3>();
    }

    /// <summary>
    /// A view over entities containing components <typeparamref name="T1"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>.
    /// <para>
    /// The view caches matching entities and rebuilds its cache when any of the underlying component pools change.
    /// Optional filters (<see cref="With{C}"/>, <see cref="Without{C}"/>) further refine results.
    /// </para>
    /// </summary>
    /// <typeparam name="T1">First required component type.</typeparam>
    /// <typeparam name="T2">Second required component type.</typeparam>
    /// <typeparam name="T3">Third required component type.</typeparam>
    public class View<T1, T2, T3> : ViewBase
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        /// <summary>
        /// Delegate invoked for each matching entity, passing refs to all three components.
        /// </summary>
        public delegate void EachDelegate(Entity entity, ref T1 component1, ref T2 component2, ref T3 component3);

        /// <summary>
        /// The three component pools that drive this view.
        /// </summary>
        private (SparseSet<T1> p1, SparseSet<T2> p2, SparseSet<T3> p3) m_Pools;

        /// <summary>
        /// Constructs a three-component view.
        /// </summary>
        public View() : base(3) { }

        /// <summary>
        /// Adds a filter requiring that entities also have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">Additional component required to pass the filter.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1, T2, T3> With<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false; // filter changes effective result set → mark cache stale
            return this;
        }

        /// <summary>
        /// Adds a filter requiring that entities do <b>not</b> have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">Component that must be absent.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1, T2, T3> Without<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => !ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        /// <summary>
        /// Determines whether the view is out of date due to version changes
        /// in any of the three component pools.
        /// </summary>
        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>() ||
                   m_Versions[1] != ComponentManager.GetVersion<T2>() ||
                   m_Versions[2] != ComponentManager.GetVersion<T3>();
        }

        /// <summary>
        /// Binds the view's pool references for <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>.
        /// </summary>
        protected override void InitializePools()
        {
            m_Pools.p1 = ComponentManager.GetPool<T1>(m_EntityManager);
            m_Pools.p2 = ComponentManager.GetPool<T2>(m_EntityManager);
            m_Pools.p3 = ComponentManager.GetPool<T3>(m_EntityManager);
        }

        /// <summary>
        /// Rebuilds the cache with entities that have all three components.
        /// Uses the smallest pool as the driver to minimize membership checks.
        /// </summary>
        protected override void PopulateCache()
        {
            // Choose the smallest pool for iteration to reduce Has() checks.
            List<ISparseSet> pools = new List<ISparseSet> { m_Pools.p1, m_Pools.p2, m_Pools.p3 };
            ISparseSet driverPool = pools.OrderBy(p => p.Size()).First();

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);

                // Set intersection across all three pools
                if (m_Pools.p1.Has(entity) && m_Pools.p2.Has(entity) && m_Pools.p3.Has(entity))
                {
                    m_Cache.Add(entity);
                }
            }
        }

        /// <summary>
        /// Captures the latest pool versions after a cache rebuild.
        /// </summary>
        protected override void UpdateLastVersions()
        {
            m_Versions[0] = ComponentManager.GetVersion<T1>();
            m_Versions[1] = ComponentManager.GetVersion<T2>();
            m_Versions[2] = ComponentManager.GetVersion<T3>();
        }

        /// <summary>
        /// Iterates all cached entities, invoking <paramref name="fn"/> with refs to their three components.
        /// Filters (if any) are applied per entity; the filter list is cleared after iteration (one-shot).
        /// </summary>
        /// <param name="fn">Delegate to execute for each matching entity.</param>
        public void Each(EachDelegate fn)
        {
            CheckAndRebuildCache();

            foreach (Entity entity in m_Cache)
            {
                // PERF: one-shot filters — if present, evaluate; otherwise straight-through
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

            // Keep semantics as "per-iteration" filters and avoid buildup.
            m_FilterPredicates.Clear();
        }
    }
}
