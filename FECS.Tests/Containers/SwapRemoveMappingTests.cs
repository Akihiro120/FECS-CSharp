using Xunit;
using FECS.Core;
using FECS.Tests.Components;

namespace FECS.Tests.Containers
{
    [Collection("FECS.Serial")]
    public class RegistrySwapRemoveMappingTests : FecsTestBase
    {
        [Fact]
        public void RemovingMiddleEntity_UpdatesBackPointers_Correctly()
        {
            var reg = new Registry();

            var e1 = reg.CreateEntity();
            var e2 = reg.CreateEntity();
            var e3 = reg.CreateEntity();

            reg.Attach(e1, new Position { X = 10, Y = 0 });
            reg.Attach(e2, new Position { X = 20, Y = 0 });
            reg.Attach(e3, new Position { X = 30, Y = 0 });

            // Remove middle (forces swap-remove from tail into the gap)
            reg.Detach<Position>(e2);

            // Remaining entities must still be present with their original values
            Assert.True(reg.Has<Position>(e1));
            Assert.True(reg.Has<Position>(e3));

            ref var p1 = ref reg.Get<Position>(e1);
            ref var p3 = ref reg.Get<Position>(e3);
            Assert.Equal(10, p1.X);
            Assert.Equal(30, p3.X);

            // A view over Position must see 2 items
            var view = reg.CreateView<Position>();
            int count = 0;
            view.Each((Entity e, ref Position p) => count++);
            Assert.Equal(2, count);
        }
    }
}

