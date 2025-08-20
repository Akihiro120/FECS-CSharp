using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    public interface ISparseSet
    {
        void Remove(Entity e);
        void Clear();
        void Size();

        EntityManager? GetEntityManager();
        void SetEntityManager(EntityManager? entityManager);
    }
}
