namespace FECS.Core
{
    public class GlobalComponent
    {
        // This is an empty component
    }

    public static class Types
    {
        public const int ENTITY_INDEX_BITS = 20;
        public const int ENTITY_VERSION_BITS = 12;

        public const uint ENTITY_INDEX_MASK = (1u << ENTITY_INDEX_BITS) - 1u;
        public const uint ENTITY_VERSION_MASK = ~ENTITY_INDEX_MASK;

        public const uint INVALID_ENTITY = uint.MaxValue;
    }
}
