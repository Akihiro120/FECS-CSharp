using Xunit;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.View
{
    [Collection("FECS.Serial")]
    public class RegistryViewMutationTests : FecsTestBase
    {
        [Fact]
        public void RemovingCurrentEntityComponent_DuringEach_DoesNotThrow_AndIsGoneNextPass()
        {
            var reg = new Registry();

            var a = reg.CreateEntity();
            var b = reg.CreateEntity();
            reg.Attach(a, new Position { X = 1, Y = 1 });
            reg.Attach(a, new Velocity { dX = 1, dY = 1 });
            reg.Attach(b, new Position { X = 2, Y = 2 });
            reg.Attach(b, new Velocity { dX = 2, dY = 2 });

            var view = reg.CreateView<Position, Velocity>();

            int visited = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) =>
            {
                visited++;
                if (e.Equals(a))
                {
                    // remove a required component on the current entity mid-iteration
                    reg.Detach<Velocity>(e);
                }
            });

            Assert.Equal(2, visited); // snapshot iteration must not throw

            // Next pass should see only 'b'
            int visitedNext = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => visitedNext++);
            Assert.Equal(1, visitedNext);
        }

        [Fact]
        public void DestroyingCurrentEntity_DuringEach_DoesNotThrow_AndIsGoneNextPass()
        {
            var reg = new Registry();

            var a = reg.CreateEntity();
            var b = reg.CreateEntity();
            reg.Attach(a, new Position { X = 1, Y = 1 });
            reg.Attach(a, new Velocity { dX = 1, dY = 1 });
            reg.Attach(b, new Position { X = 2, Y = 2 });
            reg.Attach(b, new Velocity { dX = 2, dY = 2 });

            var view = reg.CreateView<Position, Velocity>();

            int visited = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) =>
            {
                visited++;
                if (e.Equals(a))
                {
                    reg.DestroyEntity(e);
                }
            });

            Assert.Equal(2, visited); // snapshot safe

            int visitedNext = 0;
            view.Each((Entity e, ref Position p, ref Velocity v) => visitedNext++);
            Assert.Equal(1, visitedNext);
        }
    }
}

