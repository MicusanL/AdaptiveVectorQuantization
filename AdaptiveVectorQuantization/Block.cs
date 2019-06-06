using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveVectorQuantization
{

    internal class Block
    {
        public int Height { get; set; }
        public int Width { get; set; }
        internal Position Position { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public Block setupNewBlockForDictionary(Position currentBlockPosition)
        {
            Block newBlock = new Block(this);
            newBlock.Position = currentBlockPosition;

            if (Width < Height)
            {
                newBlock.Position.X--;
                newBlock.Width++;
            }
            else
            {
                newBlock.Position.Y--;
                newBlock.Height++;
            }

            return newBlock;
        }

        public Block(Position position)
        {
            this.Position = position;
            Height = 0;
            Width = 0;
        }

        public Block(Block block)
        {
            Position = block.Position;
            Height = block.Height;
            Width = block.Width;
            Size = Width * Height;
        }

        public Block(Position position, int width, int height)
        {
            Position = position;
            Height = height;
            Width = width;
            Size = Width * Height;
        }
        public override string ToString()
        {
            //return base.ToString();
            return "w: " + Width + " h: " + Height + " s: " + Size + " x: " + Position.X + " y: " + Position.Y;
        }
        public override bool Equals(Object obj)
        {
            var block = obj as Block;
            //block.height = height;
            //block.width = width;
            if (block.Position.X + Width >= AVQ.workImage.Width || block.Position.Y + Height >= AVQ.workImage.Height)
            {
                return false;
            }

            if (Position.X + Width >= AVQ.workImage.Width || Position.Y + Height >= AVQ.workImage.Height)
            {
                return false;
            }

            int blocksDiffedences = AVQ.CompareBlocks(this, block);
            if(blocksDiffedences > AVQ.Threshold)
            {
                goto label;
            }

            //for (int i = 0; i < Width; i++)
            //{
            //    for (int j = 0; j < Height; j++)
            //    {
            //        if (AVQ.ComparePx(Position.X + i, Position.Y + j, block.Position.X + i, block.Position.Y + j) == false) /* De facut clasa image? */
            //        {
            //            goto label;
            //        }
            //    }
            //}


            //Console.WriteLine(Index + " " + block.Index + " --");
            //AVQ.printBlock(this);
            //AVQ.printBlock(block);

            if(block.Height == 0 && block.Width == 0)
            {
                return true;
            }

            if (Width == block.Width && Height == block.Height)
            {
                return true;
            }
          

            label: return false;
        }


        public override int GetHashCode()
        {
            return AVQ.getPx(Position);

        }

    }
}
