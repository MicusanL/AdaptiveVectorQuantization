namespace AdaptiveVectorQuantization
{
    class Position
    {
        public override string ToString()
        {
            return X + " " + Y;
        }
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

    
        public override bool Equals(object obj)
        {
            var position = obj as Position;
            return position.X == X && position.Y == Y ;
        }

        public int X { get; set; }
        public int Y { get; set; }

    }
}
