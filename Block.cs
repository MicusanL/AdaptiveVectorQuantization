namespace AdaptiveVectorQuantization
{

    internal class Block
    {
        internal Position Position { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public int Index { get; set; }
        public int Size { get; set; }

        public Block(Position position)
        {
            Position = position;
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
            return "I: " + Index + " w: " + Width + " h: " + Height + " s: " + Size + " x: " + Position.X + " y: " + Position.Y;
        }

        public override bool Equals(object obj)
        {
            Block block = obj as Block;

            if (block.Position.X + Width >= AVQ.workImage.Width || block.Position.Y + Height >= AVQ.workImage.Height)
            {
                return false;
            }

            if (!(block.Height == 0 && block.Width == 0) && !(Width == block.Width && Height == block.Height))
            {
                return false;
            }

            if (AVQ.CompareBlocks(this, block) > AVQ.Threshold)
            {
                return false;
            }

            return true;


        }

        public override int GetHashCode()
        {
            return AVQ.getPx(Position);

        }

        public Block setupNewBlockForDictionary(Position currentBlockPosition)
        {
            Block newBlock = new Block(this)
            {
                Position = currentBlockPosition
            };

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

    }
}
