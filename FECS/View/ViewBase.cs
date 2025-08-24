using FECS.Core;
using FECS.Manager;

namespace FECS.View
{
    /// <summary>
    /// Abstract base class for ECS views.
    /// <para>
    /// A <see cref="ViewBase"/> manages cached lists of entities that satisfy
    /// certain component constraints and optional filter predicates.
    /// Derived views are responsible for specifying which component pools
    /// to query, and for implementing cache invalidation logic.
    /// </para>
    /// </summary>
    public abstract class ViewBase
    {
        /// <summary>
        /// The entity manager this view is bound to.
        /// Required for validating entity lifetimes and interacting with pools.
        /// </summary>
        protected EntityManager m_EntityManager = null!;

        /// <summary>
        /// Cache of entities currently matching this view.
        /// Rebuilt on demand when invalidated by structural changes.
        /// </summary>
        protected List<Entity> m_Cache;

        /// <summary>
        /// Stores the last-known version of each component pool relevant to this view.
        /// Used to detect changes (add/remove) that require cache rebuild.
        /// </summary>
        protected int[] m_Versions;

        /// <summary>
        /// Tracks the last-known global version (world-wide structural changes).
        /// </summary>
        protected int m_GlobalVersion = 0;

        /// <summary>
        /// Whether the cache has been built at least once.
        /// </summary>
        protected bool m_CacheBuilt = false;

        /// <summary>
        /// List of filter predicates applied to entities in this view.
        /// An entity must pass all predicates to be included in the cache.
        /// </summary>
        protected List<Func<Entity, bool>> m_FilterPredicates;

        /// <summary>
        /// Initializes a new <see cref="ViewBase"/> instance.
        /// </summary>
        /// <param name="numComponents">
        /// The number of component types this view depends on.
        /// Used to size the <see cref="m_Versions"/> tracking array.
        /// </param>
        protected ViewBase(int numComponents)
        {
            m_Cache = new List<Entity>(16); // default starting capacity
            m_Versions = new int[numComponents];
            m_FilterPredicates = new List<Func<Entity, bool>>();

            InitializePools();
        }

        /// <summary>
        /// Binds this view to a specific <see cref="EntityManager"/>.
        /// Must be called before use.
        /// </summary>
        public void SetEntityManager(EntityManager entityManager)
        {
            m_EntityManager = entityManager;
        }

        /// <summary>
        /// Ensures the entity cache has at least the given capacity.
        /// </summary>
        /// <param name="size">The number of entities to reserve space for.</param>
        public void Reserve(int size)
        {
            m_Cache.EnsureCapacity(size);
        }

        /// <summary>
        /// Evaluates whether an entity passes all registered filter predicates.
        /// </summary>
        protected bool PassesAllFilters(Entity entity)
        {
            return m_FilterPredicates.All(predicate => predicate(entity));
        }

        /// <summary>
        /// Checks whether the cache is dirty and rebuilds it if necessary.
        /// Called before enumerating entities in the view.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if no <see cref="EntityManager"/> has been assigned.
        /// </exception>
        protected void CheckAndRebuildCache()
        {
            if (m_EntityManager == null)
            {
                throw new ArgumentNullException("Entity Manager is not assigned to object.");
            }

            if (!m_CacheBuilt || IsDirty())
            {
                m_Cache.Clear();

                PopulateCache();      // Derived class populates entities matching component constraints
                UpdateLastVersions(); // Update version stamps for future IsDirty checks

                m_CacheBuilt = true;
            }

            // PERF NOTE:
            // Currently filters are applied via predicates,
            // which may force unnecessary cache invalidation.
            // Future optimization: decouple filter application from cache invalidation,
            // so filters can be added/removed without rebuilding the cache.
        }

        /// <summary>
        /// Determines whether this view’s cache is dirty
        /// (e.g. component pools have changed or global structure changed).
        /// Implemented by derived classes.
        /// </summary>
        protected abstract bool IsDirty();

        /// <summary>
        /// Initializes the component pools required by this view.
        /// Called in the constructor.
        /// </summary>
        protected abstract void InitializePools();

        /// <summary>
        /// Populates <see cref="m_Cache"/> with all entities matching this view’s component constraints.
        /// </summary>
        protected abstract void PopulateCache();

        /// <summary>
        /// Updates version tracking arrays (<see cref="m_Versions"/> and <see cref="m_GlobalVersion"/>)
        /// after a cache rebuild, so subsequent calls to <see cref="IsDirty"/> can detect changes.
        /// </summary>
        protected abstract void UpdateLastVersions();
    }
}
