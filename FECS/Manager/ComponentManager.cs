using FECS.Containers;
using FECS.Core;

namespace FECS.Manager
{
    /// <summary>
    /// Holds a static <see cref="SparseSet{T}"/> pool instance for each component type <typeparamref name="T"/>.
    /// Ensures that every component type has exactly one associated pool.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    static class PoolHolder<T>
    {
        /// <summary>
        /// The singleton sparse set instance for this component type.
        /// </summary>
        public static SparseSet<T> PoolInstance = new SparseSet<T>();
    }

    /// <summary>
    /// Maintains a version number for each component type <typeparamref name="T"/>.
    /// Used for detecting structural changes to component pools.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    static class VersionHolder<T>
    {
        /// <summary>
        /// The version counter for the component pool.
        /// Incremented whenever the pool for <typeparamref name="T"/> changes.
        /// </summary>
        public static int PoolVersion = 0;
    }

    /// <summary>
    /// Provides global management of component pools in the FECS framework.
    /// The <see cref="ComponentManager"/> ensures that each component type has a single pool,
    /// and allows global operations such as clearing all components or removing an entity
    /// across all registered pools.
    /// </summary>
    public static class ComponentManager
    {
        /// <summary>
        /// A registry of all component pools currently in use.
        /// Enables operations across all pools (e.g., deleting components for an entity).
        /// </summary>
        private static List<ISparseSet> m_RegisteredComponents = new List<ISparseSet>();

        /// <summary>
        /// Retrieves the <see cref="SparseSet{T}"/> pool for the given component type.
        /// If the pool has not yet been associated with an <see cref="EntityManager"/>,
        /// it will be registered and bound.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="entityManager">The entity manager associated with this ECS instance.</param>
        /// <returns>The singleton <see cref="SparseSet{T}"/> for this component type.</returns>
        public static SparseSet<T> GetPool<T>(EntityManager entityManager)
        {
            SparseSet<T> pool = PoolHolder<T>.PoolInstance;

            // Bind pool to entity manager if not yet initialized
            if (pool.GetEntityManager() == null)
            {
                m_RegisteredComponents.Add(pool);
                pool.SetEntityManager(entityManager);
            }

            return pool;
        }

        /// <summary>
        /// Retrieves the version number associated with the pool for a component type.
        /// The version can be incremented to track structural changes in the pool.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>A reference to the version counter.</returns>
        public static ref int GetVersion<T>()
        {
            ref int version = ref VersionHolder<T>.PoolVersion;
            return ref version;
        }

        /// <summary>
        /// Pre-allocates space in the registered component pool list.
        /// Useful for reducing allocations when many component types will be registered.
        /// </summary>
        /// <param name="size">The expected number of component types.</param>
        public static void Reserve(int size)
        {
            m_RegisteredComponents!.EnsureCapacity(size);
        }

        /// <summary>
        /// Deletes all components associated with a given entity across all registered pools.
        /// </summary>
        /// <param name="e">The entity whose components should be removed.</param>
        public static void DeleteEntity(Entity e)
        {
            foreach (ISparseSet comps in m_RegisteredComponents)
            {
                comps.Remove(e);
            }
        }

        /// <summary>
        /// Clears all registered component pools and detaches them from their <see cref="EntityManager"/>.
        /// This effectively resets the component registry.
        /// </summary>
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
