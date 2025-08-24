using FECS.Core;
using FECS.Manager;
using FECS.Containers;
using FECS.View;

namespace FECS
{
    /// <summary>
    /// Central coordination point for an FECS world.
    /// <para>
    /// The <see cref="Registry"/> owns the <see cref="EntityManager"/>,
    /// mediates access to component pools, and provides convenience APIs
    /// for entity/component operations and view creation.
    /// </para>
    /// </summary>
    public class Registry
    {
        /// <summary>
        /// Manages entity lifecycles (create/destroy/versioning).
        /// </summary>
        private EntityManager m_EntityManager;

        /// <summary>
        /// Initializes a new <see cref="Registry"/> with its own <see cref="EntityManager"/>.
        /// </summary>
        public Registry()
        {
            m_EntityManager = new EntityManager();
        }

        /// <summary>
        /// Creates a new entity within this registry and binds it to this registry
        /// so that component APIs (Attach/Get/Has/Detach) can be used directly from the entity.
        /// </summary>
        /// <returns>A new alive <see cref="Entity"/> handle.</returns>
        public Entity CreateEntity()
        {
            Entity e = m_EntityManager.Create();
            e.AttachRegistry(this); // Enable entity-bound component operations
            return e;
        }

        /// <summary>
        /// Destroys an entity and removes all of its components from every registered pool.
        /// Also bumps the global structural version (<see cref="GlobalComponent"/>) to signal world changes.
        /// </summary>
        /// <param name="id">The entity to destroy.</param>
        /// <exception cref="InvalidOperationException">Thrown if the entity is not alive.</exception>
        public void DestroyEntity(Entity id)
        {
            // Remove components across all pools first, then invalidate the entity.
            ComponentManager.DeleteEntity(id);

            // Bump a global version to signal a world-structure change (useful for systems/views cache invalidation).
            ComponentManager.GetVersion<GlobalComponent>()++;

            // Finally, invalidate the entity handle by incrementing its version via EntityManager.
            m_EntityManager.Destroy(id);
        }

        /// <summary>
        /// Checks if an entity is currently alive (valid version &amp; index).
        /// </summary>
        /// <param name="id">The entity to test.</param>
        /// <returns><c>true</c> if the entity is alive; otherwise <c>false</c>.</returns>
        public bool IsEntityAlive(Entity id)
        {
            return m_EntityManager.IsAlive(id);
        }

        /// <summary>
        /// Provides a reference to the underlying <see cref="EntityManager"/>.
        /// </summary>
        /// <returns>A reference to the internal entity manager.</returns>
        public ref EntityManager GetEntityManager()
        {
            return ref m_EntityManager;
        }

        /// <summary>
        /// Pre-reserves capacity for component pool registration bookkeeping.
        /// </summary>
        /// <param name="size">Expected number of component pools.</param>
        public void Reserve(int size)
        {
            ComponentManager.Reserve(size);
        }

        /// <summary>
        /// Ensures the component type <typeparamref name="T"/> has a registered pool for this registry.
        /// Safe to call multiple times.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        public void RegisterComponent<T>()
        {
            ComponentManager.GetPool<T>(m_EntityManager);
        }

        /// <summary>
        /// Retrieves the <see cref="SparseSet{T}"/> for the component type.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The pool managing components of type <typeparamref name="T"/>.</returns>
        public SparseSet<T> GetPool<T>()
        {
            return ComponentManager.GetPool<T>(m_EntityManager);
        }

        /// <summary>
        /// Attaches (inserts or updates) a component to the specified entity.
        /// Increments the component-type version to signal structural changes for that type.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="e">The target entity.</param>
        /// <param name="component">The component instance to attach.</param>
        public void Attach<T>(Entity e, T component)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            set.Insert(e, component);

            // Increment per-type version to notify views/systems that cache component queries.
            ComponentManager.GetVersion<T>()++;
        }

        /// <summary>
        /// Gets a reference to the component attached to an entity.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="e">The entity.</param>
        /// <returns>A reference to the component instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the entity is dead or the component does not exist.
        /// </exception>
        public ref T Get<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            return ref set.Get(e);
        }

        /// <summary>
        /// Detaches (removes) a component of type <typeparamref name="T"/> from an entity.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="e">The target entity.</param>
        public void Detach<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            set.Remove(e);

            // Increment per-type version to notify views/systems that cache component queries.
            ComponentManager.GetVersion<T>()++;
        }

        /// <summary>
        /// Checks whether an entity has a component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="e">The entity.</param>
        /// <returns><c>true</c> if present; otherwise <c>false</c>.</returns>
        public bool Has<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            return set.Has(e);
        }

        /// <summary>
        /// Gets a reference to a component of type <typeparamref name="T"/> if present;
        /// otherwise attaches the provided instance and returns a reference to it.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="e">The entity.</param>
        /// <param name="component">The component to attach if missing.</param>
        /// <returns>A reference to the existing or newly attached component.</returns>
        public ref T GetOrAttach<T>(Entity e, T component)
        {
            if (!Has<T>(e))
            {
                Attach<T>(e, component);
            }

            return ref Get<T>(e);
        }

        /// <summary>
        /// Returns a reference to the unique component of type <typeparamref name="T"/> in the world.
        /// Throws if none or more than one exists.
        /// </summary>
        /// <typeparam name="T">The component type (typically a singleton component).</typeparam>
        /// <returns>A reference to the unique component.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if zero or multiple components of type <typeparamref name="T"/> exist.
        /// </exception>
        public ref T GetSingletonComponent<T>()
            where T : struct
        {
            var pool = GetPool<T>();
            int count = pool.Size();

            if (count == 0)
                throw new InvalidOperationException($"No components of type {typeof(T).Name} exist.");
            if (count > 1)
                throw new InvalidOperationException($"Expected exactly one {typeof(T).Name}, found {count}.");

            return ref pool.Get(pool.EntityAt(0));
        }

        /// <summary>
        /// Returns the entity that owns the unique component of type <typeparamref name="T"/>.
        /// Throws if none or more than one exists.
        /// </summary>
        /// <typeparam name="T">The component type (typically a singleton component).</typeparam>
        /// <returns>The entity owning the unique component.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if zero or multiple components of type <typeparamref name="T"/> exist.
        /// </exception>
        public Entity GetSingletonEntity<T>()
            where T : struct
        {
            var pool = GetPool<T>();
            int count = pool.Size();

            if (count == 0)
                throw new InvalidOperationException($"No components of type {typeof(T).Name} exist.");
            if (count > 1)
                throw new InvalidOperationException($"Expected exactly one {typeof(T).Name}, found {count}.");

            return pool.EntityAt(0);
        }

        // ============================
        // VIEWS
        // ============================

        /// <summary>
        /// Creates a view over entities that contain components of type <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1">First component type (must be a struct for value semantics/perf).</typeparam>
        /// <returns>A cached <see cref="View{T1}"/> bound to this registry's entity manager.</returns>
        public View<T1> CreateView<T1>()
            where T1 : struct
        {
            var view = ViewHolder<T1>.ViewInstance;
            view.SetEntityManager(m_EntityManager);
            return view;
        }

        /// <summary>
        /// Creates a view over entities containing components <typeparamref name="T1"/> and <typeparamref name="T2"/>.
        /// </summary>
        /// <typeparam name="T1">First component type.</typeparam>
        /// <typeparam name="T2">Second component type.</typeparam>
        /// <returns>A cached <see cref="View{T1, T2}"/> bound to this registry.</returns>
        public View<T1, T2> CreateView<T1, T2>()
            where T1 : struct
            where T2 : struct
        {
            var view = ViewHolder<T1, T2>.ViewInstance;
            view.SetEntityManager(m_EntityManager);
            return view;
        }

        /// <summary>
        /// Creates a view over entities containing components <typeparamref name="T1"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>.
        /// </summary>
        /// <typeparam name="T1">First component type.</typeparam>
        /// <typeparam name="T2">Second component type.</typeparam>
        /// <typeparam name="T3">Third component type.</typeparam>
        /// <returns>A cached <see cref="View{T1, T2, T3}"/> bound to this registry.</returns>
        public View<T1, T2, T3> CreateView<T1, T2, T3>()
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            var view = ViewHolder<T1, T2, T3>.ViewInstance;
            view.SetEntityManager(m_EntityManager);
            return view;
        }
    }
}
