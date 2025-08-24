using Xunit;
using FECS;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.Containers
{
    [Collection("FECS.Serial")]
    public class RegistrySparseSetEdgeTests : FecsTestBase
    {
        [Fact]
        public void ManyEntities_SparseAttach_AccessHoles_OK()
        {
            var reg = new Registry();

            const int N = 5000;
            var owners = new System.Collections.Generic.List<(int idx, Entity e)>(N);

            for (int i = 0; i < N; i++)
            {
                var e = reg.CreateEntity();
                owners.Add((i, e));
                if (i % 3 == 0)
                    reg.Attach(e, new Position { X = i, Y = -i });
            }

            foreach (var (i, e) in owners)
            {
                bool shouldHave = (i % 3 == 0);
                Assert.Equal(shouldHave, reg.Has<Position>(e));
                if (shouldHave)
                {
                    ref var p = ref reg.Get<Position>(e);
                    Assert.Equal((i, -i), (p.X, p.Y));
                }
                else
                {
                    Assert.ThrowsAny<System.Collections.Generic.KeyNotFoundException>(() => reg.Get<Position>(e));
                }
            }
        }

        [Fact]
        public void GetOrAttach_ReturnsStableRef()
        {
            var reg = new Registry();
            var e = reg.CreateEntity();

            ref var p1 = ref reg.GetOrAttach(e, new Position { X = 1, Y = 2 });
            p1.X = 123;

            ref var p2 = ref reg.GetOrAttach(e, new Position { X = 9, Y = 9 });
            Assert.Equal(123, p2.X); // unchanged because component already existed
        }
    }
}
