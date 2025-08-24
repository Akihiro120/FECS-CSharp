using Xunit;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.View
{
    [Collection("FECS.Serial")]
    public class RegistryView3Tests : FecsTestBase
    {
        [Fact]
        public void Each_ThreeComponents_BasicIteration()
        {
            var reg = new Registry();

            var e = reg.CreateEntity();
            reg.Attach(e, new Position { X = 1, Y = 2 });
            reg.Attach(e, new Velocity { dX = 3, dY = 4 });
            reg.Attach(e, new Health { Value = 10 });

            var view3 = reg.CreateView<Position, Velocity, Health>();

            int count = 0;
            view3.Each((Entity e, ref Position p, ref Velocity v, ref Health h) =>
            {
                p.X += v.dX;
                p.Y += v.dY;
                h.Value += 1;
                count++;
            });

            ref var p2 = ref reg.Get<Position>(e);
            ref var h2 = ref reg.Get<Health>(e);
            Assert.Equal((4, 6), (p2.X, p2.Y));
            Assert.Equal(11, h2.Value);
            Assert.Equal(1, count);
        }
    }
}

