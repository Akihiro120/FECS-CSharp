using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    public interface ISparseSet
    {
        void Remove(Entity e);
        void Clear();

        EntityManager GetEntityManager();
        void SetEntityManager(EntityManager? entityManager);
    }
}
