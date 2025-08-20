namespace FECS.Core
{
    public sealed class Entity
    {
        private readonly uint m_ID = 0;
        private Registry? m_Registry;

        public Entity(uint index, uint version)
        {
            m_ID = (version << Types.ENTITY_INDEX_BITS) | (index & Types.ENTITY_INDEX_MASK);
        }

        public bool IsValid()
        {
            return m_ID != Types.INVALID_ENTITY;
        }

        public uint GetIndex()
        {
            return m_ID & Types.ENTITY_INDEX_MASK;
        }

        public uint GetVersion()
        {
            return (m_ID & Types.ENTITY_VERSION_MASK) >> Types.ENTITY_INDEX_BITS;
        }

        public void AttachRegistry(Registry registry)
        {
            m_Registry = registry;
        }

        public void Attach<T>(T component)
        {
            if (m_Registry == null)
            {
                throw new InvalidOperationException("Registry not attached to Entity");
            }

            m_Registry.Attach<T>(this, component);
        }

        public void Detach<T>()
        {
            if (m_Registry == null)
            {
                throw new InvalidOperationException("Registry not attached to Entity");
            }

            m_Registry.Detach<T>(this);
        }

        public ref T Get<T>()
        {
            if (m_Registry == null)
            {
                throw new InvalidOperationException("Registry not attached to Entity");
            }

            return ref m_Registry.Get<T>(this);
        }

        public ref T GetOrAttach<T>(T component)
        {
            if (m_Registry == null)
            {
                throw new InvalidOperationException("Registry not attached to Entity");
            }

            return ref m_Registry.GetOrAttach<T>(this, component);
        }

        public bool Has<T>()
        {
            if (m_Registry == null)
            {
                throw new InvalidOperationException("Registry not attached to Entity");
            }

            return m_Registry.Has<T>(this);
        }
    }
}
