using System.Runtime.InteropServices;
using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    public class SparseSet<T> : ISparseSet
    {
        private EntityManager m_EntityManager;

        private const int SPARSE_PAGE_SIZE = 2048;
        private const int NPOS = int.MaxValue;

        private List<T> m_Dense;
        private List<Entity> m_DenseEntities;
        private List<int[]?> m_Sparse;

        public SparseSet(EntityManager manager)
        {
            m_EntityManager = manager;

            m_Dense = new List<T>();
            m_DenseEntities = new List<Entity>();
            m_Sparse = new List<int[]?>();
        }

        public void Insert(Entity e, T component)
        {

        }

        public void Remove(Entity e)
        {

        }

        public bool Has(Entity e)
        {
            uint idx = e.GetIndex();
            int[]? page = PageFor((int)idx);

            if (page != null)
            {
                return page[GetPageOffset((int)idx)] != NPOS;
            }

            return false;
        }

        public ref T Get(Entity e)
        {
            if (!m_EntityManager.IsAlive(e))
            {
                throw new InvalidOperationException("Attemped Get() on a Dead Entity");
            }

            uint idx = e.GetIndex();
            int[]? page = PageFor((int)idx);
            if (page == null || page[GetPageOffset((int)idx)] == NPOS)
            {
                throw new KeyNotFoundException("Component doesn't exist for Entity");
            }

            int denseIndex = page[GetPageOffset((int)idx)];
            return ref CollectionsMarshal.AsSpan(m_Dense)[denseIndex];
        }

        public int Size()
        {
            return m_Dense.Count;
        }

        public Entity EntityAt(int idx)
        {
            return m_DenseEntities[idx];
        }

        public void Reserve(int amount)
        {
            int numPages = (amount + SPARSE_PAGE_SIZE - 1) / SPARSE_PAGE_SIZE;

            while (m_Sparse.Count < numPages)
            {
                m_Sparse.Add(null);
            }

            for (int p = 0; p < numPages; p++)
            {
                if (m_Sparse[p] == null)
                {
                    int[] page = new int[SPARSE_PAGE_SIZE];

                    // PERF: switch to using Array.Fill<> instead

                    // for (int i = 0; i < SPARSE_PAGE_SIZE; i++)
                    // {
                    //     page[i] = NPOS;
                    // }

                    // This is faster, it utilizes Buffer.MemoryCopy which can take advantage of CPU intrinsics
                    Array.Fill(page, NPOS);

                    m_Sparse[p] = page;
                }
            }

            m_Dense.EnsureCapacity(amount);
            m_DenseEntities.EnsureCapacity(amount);
        }

        public void Clear()
        {
            foreach (int[]? page in m_Sparse)
            {
                if (page != null)
                {
                    Array.Fill(page, NPOS);
                }
            }

            m_Dense.Clear();
            m_DenseEntities.Clear();
        }

        private ref int SparseSlot(int idx)
        {
            int p = GetPageIndex(idx);

            m_Sparse.EnsureCapacity(p + 1);
            while (p >= m_Sparse.Count)
            {
                m_Sparse.Add(null);
            }

            if (m_Sparse[p] == null)
            {
                int[] page = new int[SPARSE_PAGE_SIZE];
                Array.Fill(page, NPOS);
                m_Sparse[p] = page;
            }

            // We already ensured that the array isnt null, so we chill
            return ref (m_Sparse[p]!)[GetPageOffset(idx)];
        }

        private int GetPageIndex(int idx)
        {
            return idx / SPARSE_PAGE_SIZE;
        }

        private int GetPageOffset(int idx)
        {
            return idx % SPARSE_PAGE_SIZE;
        }

        private int[]? PageFor(int idx)
        {
            int p = GetPageIndex(idx);
            if (p >= m_Sparse.Count)
            {
                return null;
            }

            return m_Sparse[p];
        }
    }
}
