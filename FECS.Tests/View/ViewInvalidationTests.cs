using Xunit;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.View
{
    [Collection("FECS.Serial")]
    public class RegistryViewInvalidationTests : FecsTestBase
    {
        [Fact]
        public void View_Shrinks_WhenComponentRemoved()
        {
            var reg = new Registry();

            var e1 = reg.CreateEntity();
            reg.Attach(e1, new Position { X = 0, Y = 0 });
            reg.Attach(e1, new Velocity { dX = 1, dY = 1 });

            var e2 = reg.CreateEntity();
            reg.Attach(e2, new Position { X = 5, Y = 5 });
            reg.Attach(e2, new Velocity { dX = 2, dY = 2 });

            var view = reg.CreateView<Position, Velocity>();

            int pass1 = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => pass1++); // expect 2
            Assert.Equal(2, pass1);

            // Remove one of the required components from e2
            reg.Detach<Velocity>(e2);

            int pass2 = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => pass2++); // expect 1
            Assert.Equal(1, pass2);
        }

        [Fact]
        public void View_Shrinks_WhenEntityDestroyed()
        {
            var reg = new Registry();

            var e1 = reg.CreateEntity();
            reg.Attach(e1, new Position { X = 0, Y = 0 });
            reg.Attach(e1, new Velocity { dX = 1, dY = 1 });

            var e2 = reg.CreateEntity();
            reg.Attach(e2, new Position { X = 5, Y = 5 });
            reg.Attach(e2, new Velocity { dX = 2, dY = 2 });

            var view = reg.CreateView<Position, Velocity>();

            int pass1 = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => pass1++);
            Assert.Equal(2, pass1);

            reg.DestroyEntity(e2);

            int pass2 = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => pass2++);
            Assert.Equal(1, pass2);
        }

        [Fact]
        public void View_With_And_Without_Combined()
        {
            var reg = new Registry();

            // A: P+V+H (included)
            var a = reg.CreateEntity();
            reg.Attach(a, new Position { X = 0, Y = 0 });
            reg.Attach(a, new Velocity { dX = 1, dY = 1 });
            reg.Attach(a, new Health { Value = 10 });

            // B: P+V+H+Disabled (excluded)
            var b = reg.CreateEntity();
            reg.Attach(b, new Position { X = 0, Y = 0 });
            reg.Attach(b, new Velocity { dX = 1, dY = 1 });
            reg.Attach(b, new Health { Value = 10 });
            reg.Attach(b, new Disabled());

            // C: P+V (no Health) (excluded by With<Health>)
            var c = reg.CreateEntity();
            reg.Attach(c, new Position { X = 0, Y = 0 });
            reg.Attach(c, new Velocity { dX = 1, dY = 1 });

            var view = reg.CreateView<Position, Velocity>()
                          .With<Health>()
                          .Without<Disabled>();

            int visited = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => visited++);

            Assert.Equal(1, visited);
        }

        [Fact]
        public void View_DoesNotSeeNewEntities_MidPass()
        {
            var reg = new Registry();

            var seed = reg.CreateEntity();
            reg.Attach(seed, new Position { X = 1, Y = 1 });
            reg.Attach(seed, new Velocity { dX = 1, dY = 1 });

            var view = reg.CreateView<Position, Velocity>();

            int seen = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) =>
            {
                seen++;
                // Create a new matching entity during iteration
                var e2 = reg.CreateEntity();
                reg.Attach(e2, new Position { X = 9, Y = 9 });
                reg.Attach(e2, new Velocity { dX = 9, dY = 9 });
            });

            // A cached snapshot should not include e2 in the same pass
            Assert.Equal(1, seen);

            // Next pass should include it
            int seen2 = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => seen2++);
            Assert.Equal(2, seen2);
        }
    }
}

