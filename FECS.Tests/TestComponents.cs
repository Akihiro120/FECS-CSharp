namespace FECS.Tests.Components
{
    public struct Position
    {
        public int X;
        public int Y;
    }

    public struct Velocity
    {
        public int dX;
        public int dY;
    }

    public struct Health
    {
        public int Value;
    }

    public struct Disabled { } // tag component
}

