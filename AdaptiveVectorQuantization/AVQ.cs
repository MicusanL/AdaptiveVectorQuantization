using BitReaderWriter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AdaptiveVectorQuantization
{
    internal class AVQ
    {
        public static FastImage originalImage;
        public static FastImage workImage;
        public static bool originalImageValid;

        private const int emptyPixel = 30;
        public static bool[,] imageBitmap;
        public static int imageWidth, imageHeight;

        private static bool dictionaryChanged = false;
        private List<Position> poolGrowingPoints = new List<Position>();
        private Dictionary<Block, int> dictionary;
        private List<Block> dictionaryBackup;
        private static List<int> indexesList;

        private int currentDictionaryLength;
        private int numberBlocksFinded = 0;
        private int numberErrorPX = 0;

        public static int Threshold { get; set; }

        public static int MaxDictionaryLength { get; set; }

        private Random random = new Random();

        public AVQ(string sSourceFileName)
        {
            originalImageValid = true;
            originalImage = new FastImage(new Bitmap(sSourceFileName));

            imageBitmap = new bool[originalImage.Width, originalImage.Height];

            Bitmap blackImageBitmap = new Bitmap(sSourceFileName);
            workImage = newBlackImage(blackImageBitmap);

            dictionary = new Dictionary<Block, int>();
            dictionaryBackup = new List<Block>();
            currentDictionaryLength = 256;

            Position firstGrowingPoint = new Position(0, 0);
            poolGrowingPoints.Add(firstGrowingPoint);

        }

        public AVQ()
        {
            originalImageValid = false;
            Position firstGrowingPoint = new Position(0, 0);
            poolGrowingPoints.Add(firstGrowingPoint);

            dictionary = new Dictionary<Block, int>();
            dictionaryBackup = new List<Block>();
            currentDictionaryLength = 256;
        }


        private static void ReadFromFile(string sSourceFileName)
        {
            using (BitReader reader = new BitReader(sSourceFileName))
            {
                int indexesNumber = (int)reader.ReadNBits(32);
                Threshold = (int)reader.ReadNBits(32);
                MaxDictionaryLength = (int)reader.ReadNBits(32);
                imageWidth = (int)reader.ReadNBits(32);
                imageHeight = (int)reader.ReadNBits(32);
                imageBitmap = new bool[imageWidth, imageHeight];

                indexesList = new List<int>();

                for (int i = 0; i < indexesNumber; i++)
                {
                    int index = (int)reader.ReadNBits(32);
                    indexesList.Add(index);

                }
            }
        }

        private static void WriteInFile(List<int> indexes, bool CompressedFileFormat)
        {
            string[] fileNameParts = FormAVQ.InputFile.Split('.');
            string filenamesuffix = "_T" + Threshold + "_D" + MaxDictionaryLength;
            string outputFile = fileNameParts[0] + filenamesuffix + ".AVQ";

            if (CompressedFileFormat)
            {
                outputFile += "Comp";
                using (BitWriter writer = new BitWriter(outputFile))
                {
                    writer.WriteNBits((uint)indexes.Count, 32);
                    writer.WriteNBits((uint)Threshold, 32);
                    writer.WriteNBits((uint)MaxDictionaryLength, 32);
                    writer.WriteNBits((uint)originalImage.Width, 32);
                    writer.WriteNBits((uint)originalImage.Height, 32);
                    foreach (uint index in indexes)
                    {
                        if (index < 256)
                        {
                            writer.WriteNBits(0, 2);
                            writer.WriteNBits(index, 8);
                        }
                        else if (index < 65536)
                        {
                            writer.WriteNBits(1, 2);
                            writer.WriteNBits(index, 16);
                        }
                        else if (index < 16777216)
                        {
                            writer.WriteNBits(2, 2);
                            writer.WriteNBits(index, 24);
                        }
                        else
                        {
                            writer.WriteNBits(3, 2);
                            writer.WriteNBits(index, 32);
                        }
                    }
                }
            }
            else
            {
                using (BitWriter writer = new BitWriter(outputFile))
                {
                    writer.WriteNBits((uint)indexes.Count, 32);
                    writer.WriteNBits((uint)Threshold, 32);
                    writer.WriteNBits((uint)MaxDictionaryLength, 32);
                    writer.WriteNBits((uint)originalImage.Width, 32);
                    writer.WriteNBits((uint)originalImage.Height, 32);

                    foreach (uint index in indexes)
                    {
                        writer.WriteNBits(index, 32);
                    }
                }

            }
        }


        public static int CompareBlocks(Block b1, Block b2)
        {
            int differences = 0;
            
            for (int i = 0; i < b1.Width; i++)
            {
                for (int j = 0; j < b1.Height; j++)
                {
                    int px1, px2;
                    if (originalImageValid)
                    {
                        if (imageBitmap[b1.Position.X + i, b1.Position.Y + j] == true)
                        {
                            px1 = workImage.GetPixel(b1.Position.X + i, b1.Position.Y + j);
                        }
                        else
                        {
                            px1 = originalImage.GetPixel(b1.Position.X + i, b1.Position.Y + j);
                        }
                        if (imageBitmap[b2.Position.X + i, b2.Position.Y + j] == true)
                        {
                            px2 = workImage.GetPixel(b2.Position.X + i, b2.Position.Y + j);
                        }
                        else
                        {
                            px2 = originalImage.GetPixel(b2.Position.X + i, b2.Position.Y + j);
                        }
                    }
                    else
                    {
                        px1 = workImage.GetPixel(b1.Position.X + i, b1.Position.Y + j);
                        px2 = workImage.GetPixel(b2.Position.X + i, b2.Position.Y + j);
                    }

                    differences += Math.Abs(px1 - px2);

                }
            }

            return differences;

        }

        public static int getPx(Position position)
        {
            if (imageBitmap[position.X, position.Y] == true)
            {
                return workImage.GetPixel(position.X, position.Y);
            }
            else
            {
                if (originalImageValid)
                {
                    return originalImage.GetPixel(position.X, position.Y);
                }
                else
                {
                    return workImage.GetPixel(position.X, position.Y);
                }
            }

        }

        public static int getPx(int x, int y)
        {

            if (imageBitmap[x, y] == true)
            {
                return workImage.GetPixel(x, y);
            }
            else
            {
                if (originalImageValid)
                {
                    return originalImage.GetPixel(x, y);
                }
                else
                {
                    return workImage.GetPixel(x, y);
                }
            }
        }


        private int SearchBlock(Position position)
        {
            Block tempBlock = new Block(position);

            if (dictionary.TryGetValue(tempBlock, out int index))
            {
                return index;
            }
            else
            {
                return originalImage.GetPixel(position.X, position.Y);
            }
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

        private void DrawBlockBorder(int i, int j, Block block)
        {
            int r = random.Next(0, 255);
            int g = random.Next(0, 255);
            int b = random.Next(0, 255);

            for (int k = 0; k < block.Width; k++)
            {
                workImage.SetPixel(i + k, j, r, g, b);
                workImage.SetPixel(i + k, j + block.Height - 1, r, g, b);
            }
            for (int l = 0; l < block.Height; l++)
            {
                workImage.SetPixel(i, j + l, r, g, b);
                workImage.SetPixel(i + block.Width - 1, j + l, r, g, b);
            }
        }

        public static void PrintBlock(Block block)
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

        private void PrintImage()
        {
            for (int i = 0; i < originalImage.Width; i++)
            {
                for (int j = 0; j < originalImage.Height; j++)
                {
                    int pxValue = originalImage.GetPixel(i, j);
                    Console.Write(pxValue + " ");
                }

                Console.WriteLine();
            }
        }


        private void TryAddBlockToDictionary(Block block)
        {
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

                }
            }
            else
            {

            }
        }

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

        private Position GetNextGrowingPoint()
        {

            int minSum = int.MaxValue, index = 0;

            for (int i = 0; i < poolGrowingPoints.Count; i++)
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

        private void UpdateDictionary(Position currentPosition, int index)
        {
            if (index < 256)
            {

                Block newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), 2, 1);
                TryAddBlockToDictionary(newBlock);

                newBlock = new Block(new Position(currentPosition.X, currentPosition.Y - 1), 1, 2);
                TryAddBlockToDictionary(newBlock);

                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y - 1), 2, 2);
                TryAddBlockToDictionary(newBlock);

            }
            else
            {

                Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;

                if (findedBlock == null)
                {
                    Console.WriteLine("**");
                    foreach (Block block in dictionaryBackup)
                    {
                        if (block.Index == index)
                        {
                            findedBlock = block;
                        }
                    }

                }

                Block newBlock = new Block(new Position(currentPosition.X, currentPosition.Y - 1), findedBlock.Width, findedBlock.Height + 1);
                TryAddBlockToDictionary(newBlock);

                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y), findedBlock.Width + 1, findedBlock.Height);
                TryAddBlockToDictionary(newBlock);

                newBlock = new Block(new Position(currentPosition.X - 1, currentPosition.Y - 1), findedBlock.Width + 1, findedBlock.Height + 1);
                TryAddBlockToDictionary(newBlock);
            }

            OdrerDictionary();

        }

        private void OdrerDictionary()
        {
            if (dictionaryChanged)
            {
                Dictionary<Block, int> dic = new Dictionary<Block, int>();
                dic.Clear();

                foreach (KeyValuePair<Block, int> pair in dictionary.OrderBy(p => p.Key.Size))
                {
                    if (!dic.ContainsKey(pair.Key))
                    {
                        dic.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        dictionaryBackup.Add(pair.Key);
                    }
                }

                dictionary = dic;
                dictionaryChanged = false;
            }
        }

        private void ReplaceBlock(Block findedBlock, int i, int j)
        {
            for (int k = 0; k < findedBlock.Width; k++)
            {
                for (int l = 0; l < findedBlock.Height; l++)
                {

                    int color = workImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);

                    if (i + k < workImage.Width && j + l < workImage.Height)
                    {

                        if (imageBitmap[findedBlock.Position.X + k, findedBlock.Position.Y + l] == false)
                        {
                            color = 0;
                            numberErrorPX++;
                        }

                        workImage.SetPixel(i + k, j + l, color);
                        imageBitmap[i + k, j + l] = true;
                    }
                }
            }
        }


        public FastImage StartDeCompression(string inputFile)
        {
            ReadFromFile(inputFile);
            Bitmap blackImageBitmap = new Bitmap(imageWidth, imageHeight);//!!!!

            workImage = newBlackImage(blackImageBitmap);
            workImage.Lock();

            while (poolGrowingPoints.Count != 0)
            {
                int widthStep, heightStep;
                Position growingPoint = GetNextGrowingPoint();
                int i = growingPoint.X;
                int j = growingPoint.Y;

                int index = indexesList[0];
                indexesList.RemoveAt(0);

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    imageBitmap[i, j] = true;
                    widthStep = 1;
                    heightStep = 1;
                }
                else
                {

                    Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                    ReplaceBlock(findedBlock, i, j);

                    widthStep = findedBlock.Width;
                    heightStep = findedBlock.Height;

                    numberBlocksFinded++;
                    
                }

                TryAddGrowingPoint(new Position(i, j + heightStep));
                TryAddGrowingPoint(new Position(i + widthStep, j));

                if (i != 0 && j != 0)
                {
                    UpdateDictionary(growingPoint, index);
                }
            }
            
            Console.WriteLine("NumberBlocksFinded = {0} numberErrorPX = {1}", numberBlocksFinded, numberErrorPX);

            workImage.Unlock();
            
            return workImage;

        }

        public FastImage StartCompression(int th, int dictionarySize, bool drawBorder, bool CompressedFileFormat)
        {

            DateTime startTime = DateTime.Now;
            indexesList = new List<int>();
            Threshold = th;
            MaxDictionaryLength = dictionarySize;

            originalImage.Lock();
            workImage.Lock();

            while (poolGrowingPoints.Count != 0)
            {
                int widthStep, heightStep;
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
                }

                indexesList.Add(index);

                if (index < 256)
                {
                    workImage.SetPixel(i, j, index);
                    imageBitmap[i, j] = true;
                    widthStep = 1;
                    heightStep = 1;
                }
                else
                {

                    Block findedBlock = dictionary.LastOrDefault(x => x.Value == index).Key;
                    ReplaceBlock(findedBlock, i, j);

                    widthStep = findedBlock.Width;
                    heightStep = findedBlock.Height;


                    numberBlocksFinded++;

                    if (drawBorder)
                    {
                        DrawBlockBorder(i, j, findedBlock);
                    }
                }


                TryAddGrowingPoint(new Position(i, j + heightStep));
                TryAddGrowingPoint(new Position(i + widthStep, j));

                if (i != 0 && j != 0)
                {
                    UpdateDictionary(growingPoint, index);
                }
            }

            WriteInFile(indexesList, CompressedFileFormat);
            Console.WriteLine("NumberBlocksFinded = {0} numberErrorPX = {1}", numberBlocksFinded, numberErrorPX);

            workImage.Unlock();
            originalImage.Unlock();

            DateTime stopTime = DateTime.Now;
            Console.WriteLine(stopTime - startTime);
            return workImage;

        }

    }
}
