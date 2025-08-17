using FECS.Containers;
using FECS.Core;

namespace FECS.Manager
{
    static class PoolHolder<T>
    {
        public static SparseSet<T> PoolInstance = new SparseSet<T>();
    }

    static class VersionHolder<T>
    {
        public static int PoolVersion = 0;
    }

    public static class ComponentManager
    {
        // Singleton Object
        private static List<ISparseSet> m_RegisteredComponents = new List<ISparseSet>();

        public static SparseSet<T> GetPool<T>(EntityManager entityManager)
        {
            SparseSet<T> pool = PoolHolder<T>.PoolInstance;

            if (pool.GetEntityManager() == null)
            {
                m_RegisteredComponents.Add(pool);
                pool.SetEntityManager(entityManager);
            }

            return pool;
        }

        public static ref int GetVersion<T>()
        {
            ref int version = ref VersionHolder<T>.PoolVersion;
            return ref version;
        }

        public static void Reserve(int size)
        {
            m_RegisteredComponents!.EnsureCapacity(size);
        }

        public static void DeleteEntity(Entity e)
        {
            foreach (ISparseSet comps in m_RegisteredComponents)
            {
                comps.Remove(e);
            }
        }

        public static void ClearRegistry()
        {
            foreach (ISparseSet comps in m_RegisteredComponents)
            {
                comps.SetEntityManager(null);
                comps.Clear();
            }

            m_RegisteredComponents.Clear();
        }
    }
}
