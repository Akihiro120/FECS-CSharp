using Xunit;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.View
{
    [Collection("FECS.Serial")]
    public class RegistryView2Tests : FecsTestBase
    {
        [Fact]
        public void Each_AppliesToEntitiesWithBothComponents_AndHonorsWithout()
        {
            var reg = new Registry();

            var eA = reg.CreateEntity(); // has both, not disabled
            reg.Attach(eA, new Position { X = 0, Y = 0 });
            reg.Attach(eA, new Velocity { dX = 1, dY = 2 });

            var eB = reg.CreateEntity(); // has both, but disabled
            reg.Attach(eB, new Position { X = 10, Y = 10 });
            reg.Attach(eB, new Velocity { dX = 5, dY = 5 });
            reg.Attach(eB, new Disabled());

            var eC = reg.CreateEntity(); // only pos
            reg.Attach(eC, new Position { X = 100, Y = 100 });

            var view = reg.CreateView<Position, Velocity>().Without<Disabled>();

            int visited = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) =>
            {
                p.X += v.dX;
                p.Y += v.dY;
                visited++;
            });

            ref var aPos = ref reg.Get<Position>(eA);
            Assert.Equal((1, 2), (aPos.X, aPos.Y));

            ref var bPos = ref reg.Get<Position>(eB);
            Assert.Equal((10, 10), (bPos.X, bPos.Y));

            ref var cPos = ref reg.Get<Position>(eC);
            Assert.Equal((100, 100), (cPos.X, cPos.Y));

            Assert.Equal(1, visited);
        }

        [Fact]
        public void View_Cache_Refreshes_WhenPoolsMutate()
        {
            var reg = new Registry();

            var e1 = reg.CreateEntity();
            reg.Attach(e1, new Position { X = 0, Y = 0 });
            reg.Attach(e1, new Velocity { dX = 1, dY = 0 });

            var view = reg.CreateView<Position, Velocity>();

            int firstPass = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => firstPass++);

            // Mutate pools after first pass
            var e2 = reg.CreateEntity();
            reg.Attach(e2, new Position { X = 5, Y = 5 });
            reg.Attach(e2, new Velocity { dX = 0, dY = 1 });

            int secondPass = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => secondPass++);

            Assert.Equal(1, firstPass);
            Assert.Equal(2, secondPass);
        }
    }
}

