namespace AdaptiveVectorQuantization
{
    class Position
    {
        
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Position a, Position b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public int X { get; set; }
        public int Y { get; set; }

    }
}
