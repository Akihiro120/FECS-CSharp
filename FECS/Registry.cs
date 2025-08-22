using FECS.Core;
using FECS.Manager;
using FECS.Containers;
using FECS.View;

namespace FECS
{
    public class Registry
    {
        private EntityManager m_EntityManager;

        public Registry()
        {
            m_EntityManager = new EntityManager();
        }

        public Entity CreateEntity()
        {
            Entity e = m_EntityManager.Create();
            e.AttachRegistry(this);

            return e;
        }

        public void DestroyEntity(Entity id)
        {
            ComponentManager.DeleteEntity(id);
            ComponentManager.GetVersion<GlobalComponent>()++;
            m_EntityManager.Destroy(id);
        }

        public bool IsEntityAlive(Entity id)
        {
            return m_EntityManager.IsAlive(id);
        }

        public ref EntityManager GetEntityManager()
        {
            return ref m_EntityManager;
        }

        public void Reserve(int size)
        {
            ComponentManager.Reserve(size);
        }

        public void RegisterComponent<T>()
        {
            ComponentManager.GetPool<T>(m_EntityManager);
        }

        public SparseSet<T> GetPool<T>()
        {
            return ComponentManager.GetPool<T>(m_EntityManager);
        }

        public void Attach<T>(Entity e, T component)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            set.Insert(e, component);

            ComponentManager.GetVersion<T>()++;
        }

        public ref T Get<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            return ref set.Get(e);
        }

        public void Detach<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            set.Remove(e);
        }

        public bool Has<T>(Entity e)
        {
            SparseSet<T> set = ComponentManager.GetPool<T>(m_EntityManager);
            return set.Has(e);
        }

        public ref T GetOrAttach<T>(Entity e, T component)
        {
            if (!Has<T>(e))
            {
                Attach<T>(e, component);
            }

            return ref Get<T>(e);
        }

        // VIEWS ///////////////////////////////////////////////////
        public View<T1> CreateView<T1>()
            where T1 : struct
        {
            var view = ViewHolder<T1>.ViewInstance;
            view.SetEntityManager(m_EntityManager);

            return view;
        }

        public View<T1, T2> CreateView<T1, T2>()
            where T1 : struct
            where T2 : struct
        {
            var view = ViewHolder<T1, T2>.ViewInstance;
            view.SetEntityManager(m_EntityManager);

            return view;
        }

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
