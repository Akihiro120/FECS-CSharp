using Xunit;
using FECS;
using FECS.Tests.Components;

namespace FECS.Tests.Containers
{
    [Collection("FECS.Serial")]
    public class RegistrySparseSetTests : FecsTestBase
    {
        [Fact]
        public void Attach_Has_Get_Detach_Works()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            Assert.False(reg.Has<Position>(e));

            reg.Attach(e, new Position { X = 1, Y = 2 });
            Assert.True(reg.Has<Position>(e));

            ref var p = ref reg.Get<Position>(e);
            p.X = 42;

            ref var p2 = ref reg.Get<Position>(e);
            Assert.Equal(42, p2.X);
            Assert.Equal(2, p2.Y);

            reg.Detach<Position>(e);
            Assert.False(reg.Has<Position>(e));
        }

        [Fact]
        public void DestroyEntity_RemovesAllComponents()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            reg.Attach(e, new Position { X = 5, Y = 9 });
            reg.Attach(e, new Health { Value = 10 });
            Assert.True(reg.Has<Position>(e));
            Assert.True(reg.Has<Health>(e));

            reg.DestroyEntity(e);

            Assert.False(reg.Has<Position>(e));
            Assert.False(reg.Has<Health>(e));
            Assert.False(reg.IsEntityAlive(e));
        }

        [Fact]
        public void GetOrAttach_AttachesWhenMissing_ReturnsRef()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            ref var h1 = ref reg.GetOrAttach(e, new Health { Value = 7 });
            Assert.True(reg.Has<Health>(e));
            Assert.Equal(7, h1.Value);

            // Mutate through ref and re-read
            h1.Value = 99;
            ref var h2 = ref reg.Get<Health>(e);
            Assert.Equal(99, h2.Value);
        }
    }
}
