using FECS.Core;
using FECS.Manager;

namespace FECS.Containers
{
    /// <summary>
    /// Represents a sparse set data structure used within the FECS (Entity Component System) library.
    /// A sparse set provides efficient storage and lookup of entities,
    /// typically used for managing component associations in ECS architectures.
    /// </summary>
    public interface ISparseSet
    {
        /// <summary>
        /// Removes the specified entity from the sparse set.
        /// </summary>
        /// <param name="e">The entity to remove.</param>
        void Remove(Entity e);

        /// <summary>
        /// Clears all entities from the sparse set.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the number of entities currently stored in the sparse set.
        /// </summary>
        /// <returns>The number of entities.</returns>
        int Size();

        /// <summary>
        /// Retrieves the entity at the given index within the sparse set.
        /// </summary>
        /// <param name="idx">The index of the entity.</param>
        /// <returns>The entity at the specified index.</returns>
        Entity EntityAt(int idx);

        /// <summary>
        /// Gets the <see cref="EntityManager"/> associated with this sparse set, if any.
        /// The entity manager provides context and control over the lifecycle of entities.
        /// </summary>
        /// <returns>The associated entity manager, or <c>null</c> if not set.</returns>
        EntityManager? GetEntityManager();

        /// <summary>
        /// Sets the <see cref="EntityManager"/> to associate with this sparse set.
        /// </summary>
        /// <param name="entityManager">The entity manager to associate, or <c>null</c> to disassociate.</param>
        void SetEntityManager(EntityManager? entityManager);
    }
}

