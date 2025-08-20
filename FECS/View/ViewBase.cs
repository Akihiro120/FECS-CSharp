using FECS.Core;
using FECS.Manager;
using FECS.Containers;

namespace FECS.View
{
    public abstract class ViewBase
    {
        protected EntityManager m_EntityManager = null!;
        protected List<Entity> m_Cache;
        protected int[] m_Versions;
        protected int m_GlobalVersion = 0;
        protected bool m_CacheBuilt = false;

        protected List<Func<Entity, bool>> m_FilterPredicates;

        protected ViewBase(int numComponents)
        {
            m_Cache = new List<Entity>(16);
            m_Versions = new int[numComponents];
            m_FilterPredicates = new List<Func<Entity, bool>>();

            InitializePools();
        }

        public void SetEntityManager(EntityManager entityManager)
        {
            m_EntityManager = entityManager;
        }

        public void Reserve(int size)
        {
            m_Cache.EnsureCapacity(size);
        }

        protected bool PassesAllFilters(Entity entity)
        {
            return m_FilterPredicates.All(predicate => predicate(entity));
        }

        protected void CheckAndRebuildCache()
        {
            if (m_EntityManager == null)
            {
                throw new ArgumentNullException("Entity Manager is not assigned to object.");
            }

            if (!m_CacheBuilt || IsDirty())
            {
                m_Cache.Clear();

                PopulateCache();
                UpdateLastVersions();

                m_CacheBuilt = true;
            }

            // PERF: Potentially providing some overhead, as it only forces the cache to rebuild if filters are used.
            // Find a method to have filters, but not have to invalidate the cache
            if (m_FilterPredicates.Count > 0)
            {
                m_FilterPredicates.Clear();
                m_CacheBuilt = false;
            }
        }

        protected abstract bool IsDirty();
        protected abstract void InitializePools();
        protected abstract void PopulateCache();
        protected abstract void UpdateLastVersions();
    }
}
