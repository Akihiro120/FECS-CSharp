using Xunit;
using FECS;

namespace FECS.Tests.Core
{
    [Collection("FECS.Serial")]
    public class RegistryEntityTests : FecsTestBase
    {
        [Fact]
        public void Create_Destroy_FlagsAliveCorrectly()
        {
            var reg = new Registry();

            var e1 = reg.CreateEntity();
            var e2 = reg.CreateEntity();

            Assert.True(reg.IsEntityAlive(e1));
            Assert.True(reg.IsEntityAlive(e2));
            Assert.NotEqual(e1, e2);

            reg.DestroyEntity(e1);
            Assert.False(reg.IsEntityAlive(e1));
            Assert.True(reg.IsEntityAlive(e2));
        }

        [Fact]
        public void RecycledIndex_IsNotEqual_PreventUseAfterFree()
        {
            var reg = new Registry();
            var e1 = reg.CreateEntity();
            reg.DestroyEntity(e1);
            var e2 = reg.CreateEntity();

            Assert.NotEqual(e1, e2);
            Assert.False(reg.IsEntityAlive(e1));
            Assert.True(reg.IsEntityAlive(e2));
        }
    }
}

