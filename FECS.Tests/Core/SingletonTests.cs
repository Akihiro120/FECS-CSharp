using Xunit;
using FECS;
using FECS.Tests.Components;

namespace FECS.Tests.Core
{
    [Collection("FECS.Serial")]
    public class RegistrySingletonTests : FecsTestBase
    {
        [Fact]
        public void GetSingletonComponent_And_Entity_Works()
        {
            var reg = new Registry();

            var e = reg.CreateEntity();
            reg.Attach(e, new Health { Value = 7 });

            ref var hc = ref reg.GetSingletonComponent<Health>();
            var he = reg.GetSingletonEntity<Health>();

            Assert.Equal(7, hc.Value);
            Assert.Equal(e, he);
        }

        [Fact]
        public void GetSingletonComponent_Throws_WhenZeroOrMultiple()
        {
            var reg = new Registry();

            Assert.ThrowsAny<System.InvalidOperationException>(() => reg.GetSingletonComponent<Health>());

            var e1 = reg.CreateEntity();
            reg.Attach(e1, new Health { Value = 1 });

            _ = reg.GetSingletonComponent<Health>(); // ok with 1

            var e2 = reg.CreateEntity();
            reg.Attach(e2, new Health { Value = 2 });

            Assert.ThrowsAny<System.InvalidOperationException>(() => reg.GetSingletonComponent<Health>());
        }
    }
}

