using FECS.Manager;
using FECS.Containers;
using FECS.Core;

namespace FECS.View
{
    /// <summary>
    /// Provides a cached <see cref="View{T1}"/> singleton for reuse.
    /// Ensures that only one instance of a view exists for a given component type.
    /// </summary>
    /// <typeparam name="T1">The component type managed by the view.</typeparam>
    public static class ViewHolder<T1>
        where T1 : struct
    {
        /// <summary>
        /// The shared singleton instance of <see cref="View{T1}"/>.
        /// </summary>
        public static View<T1> ViewInstance = new View<T1>();
    }

    /// <summary>
    /// A view over entities containing exactly one component type <typeparamref name="T1"/>.
    /// <para>
    /// Views cache matching entities for iteration, automatically invalidating
    /// and rebuilding when the underlying component pool changes.
    /// Filters (<see cref="With{C}"/> / <see cref="Without{C}"/>) can further refine queries.
    /// </para>
    /// </summary>
    /// <typeparam name="T1">The component type required by this view.</typeparam>
    public class View<T1> : ViewBase
        where T1 : struct
    {
        /// <summary>
        /// Delegate type for iterating entities and their component.
        /// </summary>
        /// <param name="entity">The entity handle.</param>
        /// <param name="component1">A reference to the component instance.</param>
        public delegate void EachDelegate(Entity entity, ref T1 component1);

        /// <summary>
        /// The component pool driving this view (entities with <typeparamref name="T1"/>).
        /// </summary>
        private SparseSet<T1> m_Pools = null!;

        /// <summary>
        /// Creates a new single-component view.
        /// </summary>
        public View() : base(1) { }

        /// <summary>
        /// Adds a filter requiring that entities also have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">The component type that must also be present.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1> With<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        /// <summary>
        /// Adds a filter requiring that entities do <b>not</b> have component type <typeparamref name="C"/>.
        /// </summary>
        /// <typeparam name="C">The component type that must be absent.</typeparam>
        /// <returns>This view (for fluent chaining).</returns>
        public View<T1> Without<C>()
            where C : struct
        {
            m_FilterPredicates.Add(e => !ComponentManager.GetPool<C>(m_EntityManager).Has(e));
            m_CacheBuilt = false;
            return this;
        }

        /// <summary>
        /// Determines if the underlying component pool has changed
        /// since the last cache rebuild.
        /// </summary>
        protected override bool IsDirty()
        {
            return m_Versions[0] != ComponentManager.GetVersion<T1>();
        }

        /// <summary>
        /// Initializes the pool reference for <typeparamref name="T1"/>.
        /// Called once from the base constructor.
        /// </summary>
        protected override void InitializePools()
        {
            m_Pools = ComponentManager.GetPool<T1>(m_EntityManager);
        }

        /// <summary>
        /// Populates the cache with all entities containing <typeparamref name="T1"/>.
        /// </summary>
        protected override void PopulateCache()
        {
            ISparseSet driverPool = m_Pools;

            for (int i = 0; i < driverPool.Size(); i++)
            {
                Entity entity = driverPool.EntityAt(i);
                if (m_Pools.Has(entity)) // always true, but leaves room for custom logic
                {
                    m_Cache.Add(entity);
                }
            }
        }

        /// <summary>
        /// Updates the version tracking for this component type after a cache rebuild.
        /// </summary>
        protected override void UpdateLastVersions()
        {
            m_Versions[0] = ComponentManager.GetVersion<T1>();
        }

        /// <summary>
        /// Iterates over all entities in the view, invoking a delegate for each.
        /// <para>
        /// If filters have been applied via <see cref="With{C}"/> / <see cref="Without{C}"/>,
        /// entities must pass all filters before the delegate is invoked.
        /// </para>
        /// </summary>
        /// <param name="fn">The delegate to call for each matching entity/component pair.</param>
        public void Each(EachDelegate fn)
        {
            CheckAndRebuildCache();

            foreach (Entity entity in m_Cache)
            {
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

            // PERF: filters are currently one-shot — cleared after iteration.
            // This prevents unbounded buildup but means filters must be reapplied per query.
            m_FilterPredicates.Clear();
        }
    }
}
