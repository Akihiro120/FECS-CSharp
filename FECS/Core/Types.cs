namespace FECS.Core
{
    /// <summary>
    /// Represents a marker or tag component that can be attached to entities.
    /// A <see cref="GlobalComponent"/> contains no data and is typically used
    /// to signal global state, flags, or system-wide behaviors in an ECS architecture.
    /// </summary>
    public class GlobalComponent
    {
        // Intentionally empty: this component acts purely as a marker.
    }

    /// <summary>
    /// Defines constants and utility values used internally by the FECS framework.
    /// Provides entity encoding bit sizes, masks, and sentinel values.
    /// </summary>
    public static class Types
    {
        /// <summary>
        /// Number of bits reserved for the entity index portion of an entity ID.
        /// </summary>
        public const int ENTITY_INDEX_BITS = 20;

        /// <summary>
        /// Number of bits reserved for the entity version portion of an entity ID.
        /// Versions ensure invalid/stale entities cannot be reused incorrectly.
        /// </summary>
        public const int ENTITY_VERSION_BITS = 12;

        /// <summary>
        /// Mask used to extract the index bits from an entity ID.
        /// </summary>
        public const uint ENTITY_INDEX_MASK = (1u << ENTITY_INDEX_BITS) - 1u;

        /// <summary>
        /// Mask used to extract the version bits from an entity ID.
        /// </summary>
        public const uint ENTITY_VERSION_MASK = ~ENTITY_INDEX_MASK;

        /// <summary>
        /// Represents an invalid entity handle (all bits set).
        /// Used as a sentinel value when no valid entity is available.
        /// </summary>
        public const uint INVALID_ENTITY = uint.MaxValue;
    }
}

