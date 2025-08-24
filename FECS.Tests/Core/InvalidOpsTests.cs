using Xunit;
using FECS;
using FECS.Tests.Components;

namespace FECS.Tests.Core
{
    [Collection("FECS.Serial")]
    public class RegistryInvalidOpsTests : FecsTestBase
    {
        [Fact]
        public void Get_Throws_WhenComponentMissing()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            Assert.False(reg.Has<Position>(e));
            Assert.ThrowsAny<System.Collections.Generic.KeyNotFoundException>(() => reg.Get<Position>(e));
        }

        [Fact]
        public void Get_Throws_WhenEntityDead()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();
            reg.Attach(e, new Health { Value = 1 });

            reg.DestroyEntity(e);

            Assert.ThrowsAny<System.InvalidOperationException>(() => reg.Get<Health>(e));
        }

        [Fact]
        public void DestroyEntity_Twice_Throws()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();
            reg.DestroyEntity(e);
            Assert.ThrowsAny<System.InvalidOperationException>(() => reg.DestroyEntity(e));
        }

        [Fact]
        public void Detach_Idempotent_NoThrow()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            reg.Detach<Position>(e); // not present – should be no-op
            reg.Attach(e, new Position { X = 1, Y = 2 });
            reg.Detach<Position>(e);
            reg.Detach<Position>(e); // again no-op

            Assert.False(reg.Has<Position>(e));
        }

        [Fact]
        public void Attach_Duplicate_Updates_InPlace()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            reg.Attach(e, new Health { Value = 10 });
            reg.Attach(e, new Health { Value = 77 }); // should update, not duplicate

            var pool = reg.GetPool<Health>();
            Assert.Equal(1, pool.Size());

            ref var h = ref reg.Get<Health>(e);
            Assert.Equal(77, h.Value);
        }

        [Fact]
        public void Has_OnDeadEntity_IsFalse()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();
            reg.Attach(e, new Position { X = 1, Y = 1 });

            reg.DestroyEntity(e);

            Assert.False(reg.Has<Position>(e));
        }
    }
}
