using Xunit;
using FECS.Manager;

namespace FECS.Tests
{
    // Ensures every test starts from a clean world
    public abstract class FecsTestBase
    {
        protected FecsTestBase()
        {
            ComponentManager.ClearRegistry();
        }
    }

    // Optional xUnit collection so tests don’t run in parallel against shared statics
    [CollectionDefinition("FECS.Serial", DisableParallelization = true)]
    public class FecsSerialCollection : ICollectionFixture<FecsSerialFixture> { }

    public class FecsSerialFixture { }
}

