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

        private ArrayList poolGrowingPoints;
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

            poolGrowingPoints = new ArrayList();
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
                } catch (ArgumentException ex)
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
            for (int i = 0; i < originalImage.Width; i++)
            {
                for (int j = 0; j < originalImage.Height; j++)
                {
                    if(debugX + 1 != i || debugY + 1 != j)
                    {

                    }
                    int index;
                    Position tempPosition = new Position(i, j);

                    if (i == 0 || j == 0)
                    {
                        index = originalImage.GetPixel(i, j);
                    }
                    else
                    {
                        index = SearchBlock(tempPosition);
                        if (index < 256)
                        {
                            tempPosition.X--;
                            Block newBlock = new Block(tempPosition, 2, 1);
                            if (dictionary.ContainsKey(newBlock) == false)
                            {
                                updateDictionay(newBlock);
                            }
                        }
                        else
                        {
                            Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                            Block newBlock = findedBlock.setupNewBlockForDictionary(tempPosition);

                            if (dictionary.ContainsKey(newBlock) == false)
                            {
                                updateDictionay(newBlock);
                            }

                        }
                    }
                   
                    if (index < 256)
                    {
                        workImage.SetPixel(i, j, index);
                    }
                    else
                    {
                        if (index > dictionary.Count + 255)
                        {

                        }
                        Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                        for(int k = 0; k < findedBlock.Width; k++)
                        {
                            for(int l = 0; l < findedBlock.Height; l++)
                            {
                                int color = originalImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);
                                if(color == 255)
                                {

                                }
                                workImage.SetPixel(i + k, j + l, color);
                                debugX = i + k;
                                debugY = j + l;
                            }
                        }

            
                        if (debugX != i || debugY != j)
                        {

                        }
                    }

                }
            }

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
