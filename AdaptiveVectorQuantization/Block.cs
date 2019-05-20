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
        int index;

        Position position;

        int height;
        int width;

        public int Height { get => height; set => height = value; }
        public int Width { get => width; set => width = value; }
        internal Position Position { get => position; set => position = value; }
        public int Index { get => index; set => index = value; }

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
        }

        public Block(Position position, int height, int width)
        {
            this.Position = position;
            this.Height = height;
            this.Width = width;
        }

        public override bool Equals(Object obj)
        {
            var block = obj as Block;
            //block.height = height;
            //block.width = width;
            if (block.Position.X + Width >= AVQ.originalImage.Width || block.Position.Y + Height >= AVQ.originalImage.Height)
            {
                return false;
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (AVQ.ComparePx(Position.X + i, Position.Y + j, block.Position.X + i, block.Position.Y + j) == false) /* De facut clasa image? */
                    {
                        goto label;
                    }
                }
            }

            //if (Width == block.Width && Height == block.height)
            //{
            //    return true;
            //}
            return true;
            label: return false;
        }


        public override int GetHashCode()
        {
            return AVQ.getPx(position);

        }

    }
}
