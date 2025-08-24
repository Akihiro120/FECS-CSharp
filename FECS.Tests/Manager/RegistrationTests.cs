using Xunit;
using FECS;
using FECS.Tests.Components;

namespace FECS.Tests.Manager
{
    [Collection("FECS.Serial")]
    public class RegistryRegistrationTests : FecsTestBase
    {
        [Fact]
        public void RegisterComponent_IsIdempotent_And_PoolIsStable()
        {
            var reg = new Registry();
            reg.RegisterComponent<Position>();
            reg.RegisterComponent<Position>();

            var a = reg.GetPool<Position>();
            var b = reg.GetPool<Position>();

            Assert.Same(a, b);
        }

        [Fact]
        public void Reserve_DoesNotAffectCorrectness()
        {
            var reg = new Registry();
            reg.Reserve(16);

            var e = reg.CreateEntity();
            reg.Attach(e, new Position { X = 3, Y = 4 });
            Assert.True(reg.Has<Position>(e));
        }
    }
}


