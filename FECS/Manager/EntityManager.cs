using FECS.Core;

namespace FECS.Manager
{
    /// <summary>
    /// Manages the lifecycle of entities within the FECS framework.
    /// The <see cref="EntityManager"/> is responsible for creating,
    /// destroying, and validating entities, while maintaining versioning
    /// to ensure safety against stale references.
    /// </summary>
    public class EntityManager
    {
        /// <summary>
        /// Stores the version number for each entity index.
        /// Versions are incremented whenever an entity is destroyed,
        /// ensuring that stale <see cref="Entity"/> references become invalid.
        /// </summary>
        private List<uint> m_Versions;

        /// <summary>
        /// A freelist of previously destroyed entity indices that can be reused.
        /// Reusing indices avoids unbounded growth of the entity list.
        /// </summary>
        private List<uint> m_FreeList;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager"/> class.
        /// </summary>
        public EntityManager()
        {
            m_Versions = new List<uint>();
            m_FreeList = new List<uint>();
        }

        /// <summary>
        /// Reserves capacity for a given number of entities to minimize allocations.
        /// </summary>
        /// <param name="amount">The number of entities to reserve space for.</param>
        public void Reserve(int amount)
        {
            m_Versions.EnsureCapacity(amount);
            m_FreeList.EnsureCapacity(amount);
        }

        /// <summary>
        /// Creates a new <see cref="Entity"/>.
        /// If possible, reuses a slot from the freelist; otherwise allocates a new index.
        /// </summary>
        /// <returns>A new valid <see cref="Entity"/>.</returns>
        public Entity Create()
        {
            uint idx;

            if (m_FreeList.Count == 0)
            {
                // No free slots available → assign next index
                idx = (uint)m_Versions.Count;
                m_Versions.Add(0); // Initial version
            }
            else
            {
                // Reuse last free slot
                idx = m_FreeList[m_FreeList.Count - 1];
                m_FreeList.RemoveAt(m_FreeList.Count - 1);
            }

            return new Entity(idx, m_Versions[(int)idx]);
        }

        /// <summary>
        /// Destroys an entity, invalidating its current version
        /// and adding its index back to the freelist for reuse.
        /// </summary>
        /// <param name="e">The entity to destroy.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if attempting to destroy an already-dead entity.
        /// </exception>
        public void Destroy(Entity e)
        {
            if (!IsAlive(e))
                throw new InvalidOperationException("Attempted destruction of a dead entity.");

            uint idx = e.GetIndex();

            // Increment version → invalidates old references
            m_Versions[(int)idx]++;
            m_FreeList.Add(idx);
        }

        /// <summary>
        /// Checks whether a given entity is currently alive (valid).
        /// An entity is considered alive if its version matches
        /// the version stored in <see cref="m_Versions"/>.
        /// </summary>
        /// <param name="e">The entity to check.</param>
        /// <returns><c>true</c> if the entity is alive; otherwise <c>false</c>.</returns>
        public bool IsAlive(Entity e)
        {
            uint idx = e.GetIndex();
            uint ver = e.GetVersion();

            return (idx < m_Versions.Count && m_Versions[(int)idx] == ver);
        }
    }
}
