using FECS.Manager;
using FECS.Containers;
using FECS.Core;

namespace FECS.View
{
    /// <summary>
    /// Provides a cached <see cref="View{T1, T2}"/> singleton for reuse.
    /// Ensures only one instance per component-type pair.
    /// </summary>
    /// <typeparam name="T1">First component type.</typeparam>
    /// <typeparam name="T2">Second component type.</typeparam>
    public static class ViewHolder<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        /// <summary>
        /// The shared singleton instance of <see cref="View{T1, T2}"/>.
        /// </summary>
        public static View<T1, T2> ViewInstance = new View<T1, T2>();
    }

    /// <summary>
    /// A view over entities containing components <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// <para>
    /// The view caches matching entities and rebuilds its cache when either component pool changes.
    /// Optional filters (<see cref="With{C}"/>, <see cref="Without{C}"/>) further refine results.
    /// </para>
    /// </summary>
    /// <typeparam name="T1">First required component type.</typeparam>
    /// <typeparam name="T2">Second required component type.</typeparam>
    public class View<T1, T2> : ViewBase
        where T1 : struct
        where T2 : struct
    {
        /// <summary>
        /// Delegate invoked for each matching entity, passing refs to both components.
        /// </summary>
        public delegate void EachDelegate(Entity entity, ref T1 component1, ref T2 component2);

        /// <summary>
        /// The two component pools that drive this view.
        /// </summary>
        private (SparseSet<T1> p1, SparseSet<T2> p2) m_Pools;

        /// <summary>
        /// Constructs a two-component view.
        /// </summary>
        public View() : base(2) { }

        /// <summary>
        /// Adds a filter requiring that entities also have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">Additional component required to pass the filter.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1, T2> With<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false; // filters alter effective result set → mark cache as stale
            return this;
        }

        /// <summary>
        /// Adds a filter requiring that entities do <b>not</b> have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">Component that must be absent.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1, T2> Without<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => !ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        /// <summary>
        /// Determines whether the view is out of date due to pool version changes.
        /// </summary>
        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>() ||
                   m_Versions[1] != ComponentManager.GetVersion<T2>();
        }

        /// <summary>
        /// Binds the view's pool references.
        /// </summary>
        protected override void InitializePools()
        {
            m_Pools.p1 = ComponentManager.GetPool<T1>(m_EntityManager);
            m_Pools.p2 = ComponentManager.GetPool<T2>(m_EntityManager);
        }

        /// <summary>
        /// Rebuilds the cache with entities that have both <typeparamref name="T1"/> and <typeparamref name="T2"/>.
        /// Uses the smallest pool as a driver to minimize lookups.
        /// </summary>
        protected override void PopulateCache()
        {
            // Choose the smaller pool for iteration to reduce Has() checks.
            List<ISparseSet> pools = new() { m_Pools.p1, m_Pools.p2 };
            ISparseSet driverPool = pools.OrderBy(p => p.Size()).First();

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);

                // Confirm entity exists in both pools (set intersection)
                if (m_Pools.p1.Has(entity) && m_Pools.p2.Has(entity))
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
        }

        /// <summary>
        /// Iterates all cached entities, invoking <paramref name="fn"/> with refs to their components.
        /// Filters (if any) are applied per entity; filter list is cleared after iteration.
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
                        fn(entity, ref m_Pools.p1.Get(entity), ref m_Pools.p2.Get(entity));
                    }
                }
                else
                {
                    fn(entity, ref m_Pools.p1.Get(entity), ref m_Pools.p2.Get(entity));
                }
            }

            // Avoid unbounded growth, and keep semantics as "per-iteration" filters.
            m_FilterPredicates.Clear();
        }
    }
}
