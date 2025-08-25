using System.Runtime.InteropServices;
using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    /// <summary>
    /// A generic sparse set implementation optimized for fast insertion, removal,
    /// and lookup of components associated with entities.
    /// </summary>
    /// <typeparam name="T">The type of component stored in the sparse set.</typeparam>
    public class SparseSet<T> : ISparseSet
    {
        /// <summary>
        /// Reference to the <see cref="EntityManager"/> that owns this sparse set.
        /// Used to validate entity lifetimes and enforce ECS safety rules.
        /// </summary>
        private EntityManager? m_EntityManager;

        /// <summary>
        /// Number of entries per sparse page. This controls paging granularity for sparse lookups.
        /// </summary>
        private const int SPARSE_PAGE_SIZE = 2048;

        /// <summary>
        /// Sentinel value representing "no position". Used to mark empty sparse slots.
        /// </summary>
        private const int NPOS = int.MaxValue;

        /// <summary>
        /// Dense array of component values stored compactly.
        /// Provides O(1) iteration and cache-friendly traversal.
        /// </summary>
        private List<T> m_Dense;

        /// <summary>
        /// Dense array of entities corresponding to <see cref="m_Dense"/>.
        /// Maintains 1:1 mapping with component list.
        /// </summary>
        private List<Entity> m_DenseEntities;

        /// <summary>
        /// Sparse structure mapping entity indices to positions in <see cref="m_Dense"/>.
        /// Implemented as a paged array of int[], each page initialized with <see cref="NPOS"/>.
        /// </summary>
        private List<int[]?> m_Sparse;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseSet{T}"/> class.
        /// </summary>
        public SparseSet()
        {
            m_Dense = new List<T>();
            m_DenseEntities = new List<Entity>();
            m_Sparse = new List<int[]?>();
        }

        /// <inheritdoc/>
        public EntityManager? GetEntityManager() => m_EntityManager;

        /// <inheritdoc/>
        public void SetEntityManager(EntityManager? manager) => m_EntityManager = manager;

        /// <summary>
        /// Inserts or updates a component for the given entity.
        /// </summary>
        /// <param name="e">The target entity.</param>
        /// <param name="component">The component instance to store.</param>
        /// <exception cref="InvalidOperationException">Thrown if the entity is not alive.</exception>
        public void Insert(Entity e, T component)
        {
            if (!m_EntityManager!.IsAlive(e))
                throw new InvalidOperationException("Cannot assign component to a dead entity.");

            uint idx = e.GetIndex();
            ref int slot = ref SparseSlot((int)idx);

            if (slot == NPOS)
            {
                // New entity → append to dense arrays
                slot = m_Dense.Count;
                m_DenseEntities.Add(e);
                m_Dense.Add(component);
            }
            else
            {
                // Existing entity → overwrite component
                m_Dense[slot] = component;
            }
        }

        /// <inheritdoc/>
        public void Remove(Entity e)
        {
            if (!Has(e))
            {
                return;
            }

            if (!m_EntityManager!.IsAlive(e))
                throw new InvalidOperationException("Cannot remove component from a dead entity.");

            uint idx = e.GetIndex();
            ref int slot = ref SparseSlot((int)idx);

            int last = m_Dense.Count - 1;

            if (slot != last)
            {
                // Swap-remove last element into removed slot to keep arrays dense
                m_Dense[slot] = m_Dense[last];
                m_DenseEntities[slot] = m_DenseEntities[last];

                // Update sparse mapping for swapped entity
                SparseSlot((int)m_DenseEntities[slot].GetIndex()) = slot;
            }

            // Remove last element
            m_Dense.RemoveAt(last);
            m_DenseEntities.RemoveAt(last);
            slot = NPOS;
        }

        /// <summary>
        /// Checks whether the given entity has an associated component in this set.
        /// </summary>
        /// <param name="e">The entity to query.</param>
        /// <returns>True if the entity has a component, false otherwise.</returns>
        public bool Has(Entity e)
        {
            uint idx = e.GetIndex();
            int[]? page = PageFor((int)idx);

            if (page != null)
                return page[GetPageOffset((int)idx)] != NPOS;

            return false;
        }

        /// <summary>
        /// Retrieves a reference to the component for the given entity.
        /// </summary>
        /// <param name="e">The entity to query.</param>
        /// <returns>A reference to the stored component.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the entity is dead.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the entity has no component.</exception>
        public ref T Get(Entity e)
        {
            if (!m_EntityManager!.IsAlive(e))
                throw new InvalidOperationException("Attempted Get() on a dead entity.");

            uint idx = e.GetIndex();
            int[]? page = PageFor((int)idx);

            if (page == null || page[GetPageOffset((int)idx)] == NPOS)
                throw new KeyNotFoundException("Component does not exist for entity.");

            int denseIndex = page[GetPageOffset((int)idx)];
            return ref CollectionsMarshal.AsSpan(m_Dense)[denseIndex];
        }

        /// <inheritdoc/>
        public int Size() => m_Dense.Count;

        /// <inheritdoc/>
        public Entity EntityAt(int idx) => m_DenseEntities[idx];

        /// <summary>
        /// Reserves capacity in both dense and sparse structures.
        /// Ensures pages are allocated and initialized with <see cref="NPOS"/>.
        /// </summary>
        /// <param name="amount">The number of entities to preallocate space for.</param>
        public void Reserve(int amount)
        {
            int numPages = (amount + SPARSE_PAGE_SIZE - 1) / SPARSE_PAGE_SIZE;

            while (m_Sparse.Count < numPages)
                m_Sparse.Add(null);

            for (int p = 0; p < numPages; p++)
            {
                if (m_Sparse[p] == null)
                {
                    int[] page = new int[SPARSE_PAGE_SIZE];
                    Array.Fill(page, NPOS); // Faster than manual loop
                    m_Sparse[p] = page;
                }
            }

            m_Dense.Capacity = amount;
            m_DenseEntities.Capacity = amount;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            // Reset sparse mapping
            foreach (int[]? page in m_Sparse)
            {
                if (page != null)
                    Array.Fill(page, NPOS);
            }

            // Clear dense arrays
            m_Dense.Clear();
            m_DenseEntities.Clear();
        }

        /// <summary>
        /// Retrieves a reference to the sparse slot corresponding to an entity index.
        /// Ensures the backing page exists and is initialized.
        /// </summary>
        private ref int SparseSlot(int idx)
        {
            int p = GetPageIndex(idx);

            m_Sparse.Capacity = (p + 1);
            while (p >= m_Sparse.Count)
                m_Sparse.Add(null);

            if (m_Sparse[p] == null)
            {
                int[] page = new int[SPARSE_PAGE_SIZE];
                Array.Fill(page, NPOS);
                m_Sparse[p] = page;
            }

            return ref (m_Sparse[p]!)[GetPageOffset(idx)];
        }

        /// <summary>
        /// Gets the page index for a given entity index.
        /// </summary>
        private int GetPageIndex(int idx) => idx / SPARSE_PAGE_SIZE;

        /// <summary>
        /// Gets the offset inside a page for a given entity index.
        /// </summary>
        private int GetPageOffset(int idx) => idx % SPARSE_PAGE_SIZE;

        /// <summary>
        /// Gets the sparse page containing the slot for a given entity index.
        /// Returns <c>null</c> if the page has not been allocated.
        /// </summary>
        private int[]? PageFor(int idx)
        {
            int p = GetPageIndex(idx);
            if (p >= m_Sparse.Count)
                return null;

            return m_Sparse[p];
        }
    }
}
