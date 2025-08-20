using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    public interface ISparseSet
    {
        void Remove(Entity e);
        void Clear();
        int Size();

        EntityManager? GetEntityManager();
        void SetEntityManager(EntityManager? entityManager);
    }
}
