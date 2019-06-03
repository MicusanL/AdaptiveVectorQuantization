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

        private bool[,] imageBitmap;


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
        int numberBlocksFinded = 0;
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

        private void addGrowingPoint(Position growingPoint)
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

        private void drawBlockBorder(int i, int j, Block block)
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

        public FastImage TestDictionar()
        {
            originalImage.Lock();
            //testBuildImageMatrix();

            workImage.Lock();
            while (poolGrowingPoints.Count != 0)
            {
                Position growingPoint = poolGrowingPoints.Pop();
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
                    if (index < 256)
                    {
                        growingPoint.X--;
                        Block newBlock = new Block(growingPoint, 2, 1);
                        if (dictionary.ContainsKey(newBlock) == false)
                        {
                            updateDictionay(newBlock);
                        }
                    }
                    else
                    {
                       
                        //dictionary = dictionary.OrderBy(key => key.Key.Size).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

                        Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;


                        Block newBlock = findedBlock.setupNewBlockForDictionary(growingPoint);

                        if (dictionary.ContainsKey(newBlock) == false)
                        {
                            updateDictionay(newBlock);
                        }

                    }
                }

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    imageBitmap[i, j] = true;

                    addGrowingPoint(new Position(i, j + 1));
                    addGrowingPoint(new Position(i + 1, j));
                }
                else
                {

                    Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                    for (int k = 0; k < findedBlock.Width; k++)
                    {
                        for (int l = 0; l < findedBlock.Height; l++)
                        {
                            int color = originalImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);

                            workImage.SetPixel(i + k, j + l, color);
                            imageBitmap[i + k, j + l] = true;

                        }
                    }

                    numberBlocksFinded++;
                    drawBlockBorder(i, j, findedBlock);

                    addGrowingPoint(new Position(i, j + findedBlock.Height));
                    addGrowingPoint(new Position(i + findedBlock.Width, j));

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
