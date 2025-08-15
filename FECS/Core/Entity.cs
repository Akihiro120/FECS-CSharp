namespace FECS.Core
{
    public class Entity
    {
        private uint m_ID = 0;

        public Entity(uint index, uint version)
        {
            m_ID = (version << Types.ENTITY_INDEX_BITS) | (index & Types.ENTITY_INDEX_MASK);
        }

        public bool IsValid()
        {
            return m_ID == Types.INVALID_ENTITY;
        }

        public uint GetIndex()
        {
            return m_ID & Types.ENTITY_INDEX_MASK;
        }

        public uint GetVersion()
        {
            return (m_ID & Types.ENTITY_VERSION_MASK) >> Types.ENTITY_INDEX_BITS;
        }
    }
}
