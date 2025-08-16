using FECS.Core;

namespace FECS.Manager
{
    public class EntityManager
    {
        private List<uint> m_Versions;
        private List<uint> m_FreeList;

        public EntityManager()
        {
            m_Versions = new List<uint>();
            m_FreeList = new List<uint>();
        }

        public void Reserve(int amount)
        {
            m_Versions.EnsureCapacity(amount);
            m_FreeList.EnsureCapacity(amount);
        }

        public Entity Create()
        {
            uint idx = 0;

            if (m_FreeList.Count == 0)
            {
                idx = (uint)m_Versions.Count;
                m_Versions.Add(0);
            }
            else
            {
                idx = m_FreeList[m_FreeList.Count - 1];
                m_FreeList.RemoveAt(m_FreeList.Count - 1);
            }

            return new Entity(idx, m_Versions[(int)idx]);
        }

        public void Destroy(Entity e)
        {
            // Prevent the destruction of a dead entity;
            if (!IsAlive(e))
            {
                throw new InvalidOperationException("Attempted Destruction of a Dead Entity.");
            }

            uint idx = e.GetIndex();
            m_Versions[(int)idx]++;
            m_FreeList.Add(idx);
        }

        public bool IsAlive(Entity e)
        {
            uint idx = e.GetIndex();
            uint ver = e.GetVersion();

            return (idx < m_Versions.Count && m_Versions[(int)idx] == ver);
        }
    }
}
