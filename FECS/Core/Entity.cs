namespace FECS.Core
{
    /// <summary>
    /// Represents an entity in the FECS framework.
    /// An <see cref="Entity"/> is a lightweight handle consisting of an index and version,
    /// encoded into a single <see cref="uint"/> ID. Entities can attach to a <see cref="Registry"/>
    /// in order to manage components.
    /// </summary>
    public sealed class Entity
    {
        /// <summary>
        /// Packed entity ID containing both index and version.
        /// Layout:
        /// [ Version (12 bits) | Index (20 bits) ]
        /// </summary>
        private readonly uint m_ID = 0;

        /// <summary>
        /// Optional reference to the <see cref="Registry"/> this entity is bound to.
        /// Provides access to component operations.
        /// </summary>
        private Registry? m_Registry;

        /// <summary>
        /// Constructs a new <see cref="Entity"/> with the given index and version.
        /// The two values are encoded into a single 32-bit integer.
        /// </summary>
        /// <param name="index">The entity index (lower bits).</param>
        /// <param name="version">The entity version (upper bits).</param>
        public Entity(uint index, uint version)
        {
            m_ID = (version << Types.ENTITY_INDEX_BITS) | (index & Types.ENTITY_INDEX_MASK);
        }

        /// <summary>
        /// Checks whether this entity represents a valid handle.
        /// </summary>
        /// <returns>True if valid; otherwise false.</returns>
        public bool IsValid()
        {
            return m_ID != Types.INVALID_ENTITY;
        }

        /// <summary>
        /// Gets the index portion of this entity ID.
        /// </summary>
        /// <returns>The entity index.</returns>
        public uint GetIndex()
        {
            return m_ID & Types.ENTITY_INDEX_MASK;
        }

        /// <summary>
        /// Gets the version portion of this entity ID.
        /// Versions protect against stale handles to destroyed entities.
        /// </summary>
        /// <returns>The entity version.</returns>
        public uint GetVersion()
        {
            return (m_ID & Types.ENTITY_VERSION_MASK) >> Types.ENTITY_INDEX_BITS;
        }

        /// <summary>
        /// Attaches this entity to a <see cref="Registry"/> to enable component operations.
        /// </summary>
        /// <param name="registry">The registry to attach to.</param>
        public void AttachRegistry(Registry registry)
        {
            m_Registry = registry;
        }

        /// <summary>
        /// Attaches a component of type <typeparamref name="T"/> to this entity.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="component">The component instance to attach.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no <see cref="Registry"/> is attached.
        /// </exception>
        public void Attach<T>(T component)
        {
            if (m_Registry == null)
                throw new InvalidOperationException("Registry not attached to Entity");

            m_Registry.Attach(this, component);
        }

        /// <summary>
        /// Detaches a component of type <typeparamref name="T"/> from this entity.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no <see cref="Registry"/> is attached.
        /// </exception>
        public void Detach<T>()
        {
            if (m_Registry == null)
                throw new InvalidOperationException("Registry not attached to Entity");

            m_Registry.Detach<T>(this);
        }

        /// <summary>
        /// Retrieves a reference to a component of type <typeparamref name="T"/> attached to this entity.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>A reference to the component.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no <see cref="Registry"/> is attached.
        /// </exception>
        public ref T Get<T>()
        {
            if (m_Registry == null)
                throw new InvalidOperationException("Registry not attached to Entity");

            return ref m_Registry.Get<T>(this);
        }

        /// <summary>
        /// Retrieves a reference to a component of type <typeparamref name="T"/> attached to this entity,
        /// or attaches the component if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="component">The component instance to attach if missing.</param>
        /// <returns>A reference to the component.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no <see cref="Registry"/> is attached.
        /// </exception>
        public ref T GetOrAttach<T>(T component)
        {
            if (m_Registry == null)
                throw new InvalidOperationException("Registry not attached to Entity");

            return ref m_Registry.GetOrAttach(this, component);
        }

        /// <summary>
        /// Checks whether this entity currently has a component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>True if the entity has the component; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no <see cref="Registry"/> is attached.
        /// </exception>
        public bool Has<T>()
        {
            if (m_Registry == null)
                throw new InvalidOperationException("Registry not attached to Entity");

            return m_Registry.Has<T>(this);
        }
    }
}
