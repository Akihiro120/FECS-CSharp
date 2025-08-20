using FECS.Core;
using FECS.Manager;

namespace FECS.View
{
    public abstract class ViewBase
    {
        protected EntityManager m_EntityManager = null!;
        protected List<Entity> m_Cache;
        protected int[] m_Versions;
        protected int m_GlobalVersion = 0;
        protected bool m_CacheBuilt = false;

        protected ViewBase(int numComponents)
        {
            m_Cache = new List<Entity>();
            m_Versions = new int[numComponents];

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
        }

        protected abstract bool IsDirty();
        protected abstract void InitializePools();
        protected abstract void PopulateCache();
        protected abstract void UpdateLastVersions();
    }
}
