using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace AdaptiveVectorQuantization
{
    internal class AVQ
    {//test
        private const int emptyPixel = 255;
        private static bool dictionaryChanged = false;
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
        private List<Position> poolGrowingPoints = new List<Position>();
        private Dictionary<Block, int> dictionary;
        private int currentDictionaryLength; /* currentDictionaryLength = [256; maxDictionaryLength] */
        private int numberBlocksFinded = 0;

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
            poolGrowingPoints.Add(firstGrowingPoint);

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
            //for (int k = 0; k < block.Width; k++)
            //{
            //    for (int l = 0; l < block.Height; l++)
            //    {
            //        int x = block.Position.X + k;
            //        int y = block.Position.Y + l;
            //        if (imageBitmap[block.Position.X + k, block.Position.Y + l] == false)
            //        {
            //            return;
            //        }
            //    }
            //}
            if (currentDictionaryLength < MaxDictionaryLength)
            {

                try
                {
                    block.Index = currentDictionaryLength;

                    dictionary.Add(block, currentDictionaryLength++);
                    dictionaryChanged = true;
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
            if (poolGrowingPoints.Contains(growingPoint))
            {
                return;
            }

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

        addGP: poolGrowingPoints.Add(growingPoint);
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

        private Position GetNextGrowingPoint()
        {
            //int indexMax = !poolGrowingPoints.Any() ? -1 :
            //        poolGrowingPoints.Select((value, index) => new { Value = value, Index = index })
            //                        .Aggregate((a, b) => (a.Value.X + a.Value.Y < b.Value.X + b.Value.Y) ? a : b)
            //                        .Index;

            //poolGrowingPoints.OrderBy(gp => (gp.X + gp.Y));

            int minSum = int.MaxValue, index = 0;

            for(int i = 0; i < poolGrowingPoints.Count; i++)
            {
                if (poolGrowingPoints[i].X + poolGrowingPoints[i].Y < minSum)
                {
                    minSum = poolGrowingPoints[i].X + poolGrowingPoints[i].Y;
                    index = i;
                }
            }
           

            Position growingPoint = poolGrowingPoints[index];
            poolGrowingPoints.RemoveAt(index);
            return growingPoint;
        }



        private static void WriteInFile(List<int> indexes)
        {
            //using (BinaryWriter writer = new BinaryWriter(File.Open("exitFile", FileMode.Create)))
            //{
            //    writer.Write(index);
            //}

            using (FileStream fs = new FileStream("exitFile", FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, indexes);
            }
        }

        public static void printBlock(Block block)
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

        private void UpdateDictionary(Position currentPosition, int index)
        {
            if (index < 256)
            {


                Block newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), 2, 1);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }

                 
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
                
            }
            else
            {
                if (dictionaryChanged)
                {
                    dictionary = dictionary.OrderBy(key => key.Key.Size).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                    dictionaryChanged = false;
                }

                Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;


                Block newBlock = new Block(new Position(currentPosition.X, currentPosition.Y - 1), findedBlock.Width, findedBlock.Height + 1);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }
                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), findedBlock.Width + 1, findedBlock.Height);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }
                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y - 1), findedBlock.Width + 1, findedBlock.Height + 1);
                if (dictionary.ContainsKey(newBlock) == false)
                {
                    TryAddBlockToDictionary(newBlock);
                }

            }
        }

        public FastImage StartCompression(int th, int dictionarySize, bool drawBorder)
        {
            List<int> indexesList = new List<int>();

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

                indexesList.Add(index);
                //WriteInFile(index);

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    imageBitmap[i, j] = true;
                    //Console.WriteLine("{0} {1}", i, j);

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

                            int color = workImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);
                            if (imageBitmap[findedBlock.Position.X + k, findedBlock.Position.Y + l] == false)
                            {

                            }
                            workImage.SetPixel(i + k, j + l, color);
                            imageBitmap[i + k, j + l] = true;
                            //Console.WriteLine("{0} {1}", i + k, j + l);

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

            WriteInFile(indexesList);
            Console.WriteLine("NumberBlocksFinded = {0}", numberBlocksFinded);

            workImage.Unlock();
            originalImage.Unlock();
            return workImage;

        }

        public FastImage Test()
        {
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
