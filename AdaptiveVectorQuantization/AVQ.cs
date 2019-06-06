using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace AdaptiveVectorQuantization
{

    class AVQ
    {//test
        private const int emptyPixel = 255;

        public static FastImage originalImage;
        public static FastImage workImage;
        public static bool[,] imageBitmap;

        public static int CompareBlocks(Block b1, Block b2)
        {
            int differences = 0;

            //printBlock(b1);
            //printBlock(b2);
            //Console.WriteLine("--  " + b1.Width + " " + b1.Height);
            for (int i = 0; i < b1.Width; i++)
            {
                for (int j = 0; j < b1.Height; j++)
                {
                    int px1 = originalImage.GetPixel(b1.Position.X + i, b1.Position.Y + j);
                    int px2 = originalImage.GetPixel(b2.Position.X + i, b2.Position.Y + j);
                    //Console.WriteLine(px1 + " " + px2);
                    //if(Math.Abs(px1 - px2) > 200)
                    //{
                    //    differences = Int16.MaxValue;
                    //    return differences;
                    //}
                    differences += Math.Abs(px1 - px2);

                }
            }

            return differences;

        }
        //private static bool ComparePx(int x1, int y1, int x2, int y2)
        //{

        //    if (originalImage.GetPixel(x1, y1) == originalImage.GetPixel(x2, y2))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static int getPx(Position position)
        {
            return originalImage.GetPixel(position.X, position.Y);
        }

        public static int getPx(int x, int y)
        {
            return originalImage.GetPixel(x, y);
        }

        //private ArrayList poolGrowingPoints;
        private Stack<Position> poolGrowingPoints = new Stack<Position>();
        private Dictionary<Block, int> dictionary;
        private int currentDictionaryLength; /* currentDictionaryLength = [256; maxDictionaryLength] */
        int numberBlocksFinded = 0;

        public static int Threshold { get; set; }
        public int MaxDictionaryLength { get; set; }

        private int SearchBlock(Position position)
        {
            Block tempBlock = new Block(position);

            if (dictionary.TryGetValue(tempBlock, out int index)) /* see override Equals from Block */
            {
                return index;
            }
            else
            {
                return originalImage.GetPixel(position.X, position.Y);
            }
        }

        public AVQ(string sSourceFileName)
        {

            originalImage = new FastImage(new Bitmap(sSourceFileName));

            imageBitmap = new bool[originalImage.Width, originalImage.Height];

            Bitmap blackImageBitmap = new Bitmap(sSourceFileName);
            workImage = newBlackImage(blackImageBitmap);

            dictionary = new Dictionary<Block, int>();
            currentDictionaryLength = 256;

            Position firstGrowingPoint = new Position(0, 0);
            poolGrowingPoints.Push(firstGrowingPoint);

        }

        private FastImage newBlackImage(Bitmap imageBitmap)
        {
            FastImage newImg = new FastImage(imageBitmap);

            newImg.Lock();
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    newImg.SetPixel(i, j, emptyPixel);
                }
            }

            newImg.Unlock();

            return newImg;
        }


        private void TryAddBlockToDictionary(Block block)
        {
            if (currentDictionaryLength < MaxDictionaryLength)
            {
                try
                {
                    block.Index = currentDictionaryLength;

                    dictionary.Add(block, currentDictionaryLength++);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("##");
                }
            }
            else
            {

            }
        }
        //private int[,] imageMatrix;
        //private void testBuildImageMatrix()
        //{
        //    imageMatrix = new int[originalImage.Width, originalImage.Height];
        //    for (int i = 0; i < originalImage.Width; i++)
        //    {
        //        for (int j = 0; j < originalImage.Height; j++)
        //        {
        //            imageMatrix[i, j] = originalImage.GetPixel(i, j);
        //        }
        //    }
        //}

        private void TryAddGrowingPoint(Position growingPoint)
        {
            if (growingPoint.X >= workImage.Width || growingPoint.Y >= workImage.Height)
            {
                return;
            }

            if (imageBitmap[growingPoint.X, growingPoint.Y] == true)
            {
                return;
            }

            if (growingPoint.X <= 1 || growingPoint.Y <= 1)
            {
                goto addGP;
            }

            if (imageBitmap[growingPoint.X - 1, growingPoint.Y - 1] == false &&
                imageBitmap[growingPoint.X, growingPoint.Y - 1] == false &&
                imageBitmap[growingPoint.X - 1, growingPoint.Y] == false)
            {
                return;
            }

            addGP: poolGrowingPoints.Push(growingPoint);
        }

        private void DrawBlockBorder(int i, int j, Block block)
        {
            for (int k = 0; k < block.Width; k++)
            {
                workImage.SetPixel(i + k, j, 255);
                workImage.SetPixel(i + k, j + block.Height - 1, 255);
            }
            for (int l = 0; l < block.Height; l++)
            {
                workImage.SetPixel(i, j + l, 255);
                workImage.SetPixel(i + block.Width - 1, j + l, 255);
            }
        }

        Position GetNextGrowingPoint()
        {
            Position growingPoint = poolGrowingPoints.Pop();
            return growingPoint;
        }

        int ChoodeBlockFromDictionary(Position growingPoint)
        {
            int index = 0;

            return index;
        }

        static public void printBlock(Block block)
        {
            for (int i = block.Position.X; i < block.Position.X + block.Width; i++)
            {
                for (int j = block.Position.Y; j < block.Position.Y + block.Height; j++)
                {
                    Console.Write(getPx(i, j));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        void UpdateDictionary(Position currentPosition, int index)
        {
            if (index < 256)
            {


                Block newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), 2, 1);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }

                /* 
                newBlock = new Block(new Position(currentPosition.X, currentPosition.Y - 1), 1, 2);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }

                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y - 1), 2, 2);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }
                */
            }
            else
            {

                dictionary = dictionary.OrderBy(key => key.Key.Size).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);


                Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;


                //Block newBlock = new Block(new Position(currentPosition.X, currentPosition.Y - 1), findedBlock.Width, findedBlock.Height + 1);
                //if (dictionary.ContainsKey(newBlock) == false)
                //{
                //    TryAddBlockToDictionary(newBlock);
                //}
                //newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), findedBlock.Width + 1, findedBlock.Height);
                //if (dictionary.ContainsKey(newBlock) == false)
                //{
                //    TryAddBlockToDictionary(newBlock);
                //}
                //newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y - 1), findedBlock.Width + 1, findedBlock.Height + 1);
                //if (dictionary.ContainsKey(newBlock) == false)
                //{
                //    TryAddBlockToDictionary(newBlock);
                //}

            }
        }

        public FastImage TestDictionar(int th, int dictionarySize, bool drawBorder)
        {
            Threshold = th;
            MaxDictionaryLength = dictionarySize;

            originalImage.Lock();
            //testBuildImageMatrix();

            workImage.Lock();
            while (poolGrowingPoints.Count != 0)
            {
                Position growingPoint = GetNextGrowingPoint();
                int i = growingPoint.X;
                int j = growingPoint.Y;
                int index;

                if (i == 0 || j == 0)
                {
                    index = originalImage.GetPixel(i, j);
                }
                else
                {
                    index = SearchBlock(growingPoint);
                    UpdateDictionary(growingPoint, index);
                }

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    imageBitmap[i, j] = true;

                    TryAddGrowingPoint(new Position(i, j + 1));
                    TryAddGrowingPoint(new Position(i + 1, j));
                }
                else
                {

                    Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                    for (int k = 0; k < findedBlock.Width; k++)
                    {
                        for (int l = 0; l < findedBlock.Height; l++)
                        {
                            if(imageBitmap[findedBlock.Position.X + k, findedBlock.Position.Y + l] == false)
                            {

                            }
                            int color = workImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);
                            if(color == 255)
                            {

                            }
                            workImage.SetPixel(i + k, j + l, color);
                            imageBitmap[i + k, j + l] = true;

                        }
                    }

                    TryAddGrowingPoint(new Position(i, j + findedBlock.Height));
                    TryAddGrowingPoint(new Position(i + findedBlock.Width, j));

                    numberBlocksFinded++;

                    if (drawBorder)
                    {
                        DrawBlockBorder(i, j, findedBlock);
                    }
                }
            }

            Console.WriteLine("NumberBlocksFinded = {0}", numberBlocksFinded);

            workImage.Unlock();
            originalImage.Unlock();
            return workImage;

        }

        public FastImage Test()
        {
            Color color;

            originalImage.Lock();
            for (int i = 0; i < originalImage.Width; i++)
            {
                for (int j = 0; j < originalImage.Height; j++)
                {
                    int pxValue = originalImage.GetPixel(i, j);

                    int newPxValue = pxValue;

                    originalImage.SetPixel(i, j, newPxValue);
                }
            }

            originalImage.Unlock();
            return originalImage;

        }
    }
}
