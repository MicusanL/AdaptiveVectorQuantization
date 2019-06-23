using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveVectorQuantization
{
    class GrowingPoint
    {
        public override string ToString()
        {
            return X + " " + Y;
        }
        public GrowingPoint(int x, int y)
        {
            X = x;
            Y = y;
            Sum = x + y;
        }


        public override bool Equals(object obj)
        {
            var position = obj as Position;
            return position.X == X && position.Y == Y;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Sum { get; set; }

    }
}
