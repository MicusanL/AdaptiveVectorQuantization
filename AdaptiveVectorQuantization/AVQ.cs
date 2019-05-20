using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace AdaptiveVectorQuantization
{




    //class Dictionary
    //{
    //    int index;
    //    Block block;
    //}


    class AVQ
    {
        public static FastImage originalImage;
        public static FastImage workImage;


        public static bool ComparePx(int x1, int y1, int x2, int y2)
        {
            if (originalImage.GetPixel(x1, y1) == originalImage.GetPixel(x2, y2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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
        private const int maxDictionaryLength = 1000;
        private int currentDictionaryLength; /* currentDictionaryLength = [256; maxDictionaryLength] */

        private int SearchBlock(Position position)
        {
            Block tempBlock = new Block(position);

            if (dictionary.TryGetValue(tempBlock, out int index)) /* see override Equals from Block */
            {
                //Console.WriteLine(index);
                return index;
            }
            else
            {
                return originalImage.GetPixel(position.X, position.Y);
            }
        }

        public AVQ(string sSourceFileName)
        {

            Bitmap imageBitmap = new Bitmap(sSourceFileName);

            originalImage = new FastImage(imageBitmap);

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
                    newImg.SetPixel(i, j, 255);
                }
            }
            newImg.Unlock();

            return newImg;
        }


        private void updateDictionay(Block block)
        {
            if (currentDictionaryLength < maxDictionaryLength)
            {
                try
                {
                    block.Index = currentDictionaryLength;
                    dictionary.Add(block, currentDictionaryLength++);
                }
                catch (ArgumentException ex)
                {
                    int key;
                    dictionary.TryGetValue(block, out key);//!!!!!!!!!!!!!!
                                                           // Console.WriteLine(key);
                }
            }
            else
            {

            }
        }
        private int[,] imageMatrix;
        private void testBuildImageMatrix()
        {
            imageMatrix = new int[originalImage.Width, originalImage.Height];
            for (int i = 0; i < originalImage.Width; i++)
            {
                for (int j = 0; j < originalImage.Height; j++)
                {
                    imageMatrix[i, j] = originalImage.GetPixel(i, j);
                }
            }
        }

        public FastImage TestDictionar()
        {
            // Color color;
            int debugX = 0, debugY = 0;

            originalImage.Lock();
            testBuildImageMatrix();

            workImage.Lock();
            while (poolGrowingPoints.Count != 0)
            {
                Position currentPosition = poolGrowingPoints.Pop();
                int i = currentPosition.X;
                int j = currentPosition.Y;
                int index;

                if (i == 0 || j == 0)
                {
                    index = originalImage.GetPixel(i, j);
                }
                else
                {
                    index = SearchBlock(currentPosition);
                    if (index < 256)
                    {
                        currentPosition.X--;
                        Block newBlock = new Block(currentPosition, 2, 1);
                        if (dictionary.ContainsKey(newBlock) == false)
                        {
                            updateDictionay(newBlock);
                        }
                    }
                    else
                    {
                        Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                        Block newBlock = findedBlock.setupNewBlockForDictionary(currentPosition);

                        if (dictionary.ContainsKey(newBlock) == false)
                        {
                            updateDictionay(newBlock);
                        }

                    }
                }

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    //if(new j< maxJ)
                    Position newGrowingPoint1 = new Position(i, j + 1);
                    if (newGrowingPoint1.Y < originalImage.Height && ((newGrowingPoint1.X - 1 >= 0) ? (workImage.GetPixel(newGrowingPoint1.X - 1, newGrowingPoint1.Y) != 255) : true))
                    {
                        poolGrowingPoints.Push(newGrowingPoint1);
                    }

                    Position newGrowingPoint2 = new Position(i + 1, j);
                    if (newGrowingPoint2.X < originalImage.Width && ((newGrowingPoint2.Y - 1 >= 0) ? (workImage.GetPixel(newGrowingPoint2.X, newGrowingPoint2.Y - 1) != 255) : true))
                    {
                        poolGrowingPoints.Push(newGrowingPoint2);
                    }
                }
                else
                {
                    if (index > dictionary.Count + 255)
                    {

                    }
                    Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                    for (int k = 0; k < findedBlock.Width; k++)
                    {
                        for (int l = 0; l < findedBlock.Height; l++)
                        {
                            int color = originalImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);
                            if (color == 255)
                            {

                            }
                            workImage.SetPixel(i + k, j + l, color);
                            //debugX = i + k;
                            //debugY = j + l;
                        }
                    }

                    Position newGrowingPoint1 = new Position(i, j + findedBlock.Height);
                    if (newGrowingPoint1.Y < originalImage.Height && ((newGrowingPoint1.X - 1 >= 0) ? (workImage.GetPixel(newGrowingPoint1.X - 1, newGrowingPoint1.Y) != 255) : true))
                    {
                        poolGrowingPoints.Push(newGrowingPoint1);
                    }

                    Position newGrowingPoint2 = new Position(i + findedBlock.Width, j);
                    if (newGrowingPoint2.X < originalImage.Width && ((newGrowingPoint2.Y - 1 >= 0) ? (workImage.GetPixel(newGrowingPoint2.X, newGrowingPoint2.Y - 1) != 255) : true))
                    {
                        poolGrowingPoints.Push(newGrowingPoint2);
                    }


                    //if (debugX != i || debugY != j)
                    //{

                    //}
                }
            }

            //for (int i = 0; i < originalImage.Width; i++)
            //{
            //    for (int j = 0; j < originalImage.Height; j++)
            //    {
            //        if (debugX + 1 != i || debugY + 1 != j)
            //        {

            //        }
            //        int index;
            //        Position tempPosition = new Position(i, j);

            //        if (i == 0 || j == 0)
            //        {
            //            index = originalImage.GetPixel(i, j);
            //        }
            //        else
            //        {
            //            index = SearchBlock(tempPosition);
            //            if (index < 256)
            //            {
            //                tempPosition.X--;
            //                Block newBlock = new Block(tempPosition, 2, 1);
            //                if (dictionary.ContainsKey(newBlock) == false)
            //                {
            //                    updateDictionay(newBlock);
            //                }
            //            }
            //            else
            //            {
            //                Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
            //                Block newBlock = findedBlock.setupNewBlockForDictionary(tempPosition);

            //                if (dictionary.ContainsKey(newBlock) == false)
            //                {
            //                    updateDictionay(newBlock);
            //                }

            //            }
            //        }

            //        if (index < 256)
            //        {
            //            workImage.SetPixel(i, j, index);
            //        }
            //        else
            //        {
            //            if (index > dictionary.Count + 255)
            //            {

            //            }
            //            Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
            //            for (int k = 0; k < findedBlock.Width; k++)
            //            {
            //                for (int l = 0; l < findedBlock.Height; l++)
            //                {
            //                    int color = originalImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);
            //                    if (color == 255)
            //                    {

            //                    }
            //                    workImage.SetPixel(i + k, j + l, color);
            //                    debugX = i + k;
            //                    debugY = j + l;
            //                }
            //            }


            //            if (debugX != i || debugY != j)
            //            {

            //            }
            //        }

            //    }
            //}

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
