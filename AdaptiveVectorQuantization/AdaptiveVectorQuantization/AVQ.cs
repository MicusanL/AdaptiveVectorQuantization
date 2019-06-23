using BitReaderWriter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AdaptiveVectorQuantization
{
    internal class AVQ
    {
        public static FastImage originalImage;
        public static FastImage workImage;
        public static FastImage bitmapBlocks;
        public static bool originalImageValid;

        private const int emptyPixel = 0;
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
        public static int BlocPainFrequency { get; set; }

        private Random random = new Random();

        public AVQ(string sSourceFileName)
        {
            originalImageValid = true;
            originalImage = new FastImage(new Bitmap(sSourceFileName));

            imageBitmap = new bool[originalImage.Width, originalImage.Height];

            Bitmap blackImageBitmap = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format24bppRgb);
            Bitmap blackImageBitmap2 = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format24bppRgb);
            workImage = new FastImage(blackImageBitmap);
            bitmapBlocks = new FastImage(blackImageBitmap2);

            dictionary = new Dictionary<Block, int>();
            dictionaryBackup = new List<Block>();
            currentDictionaryLength = 256;

            //Position firstGrowingPoint = new Position(0, 0);
            //poolGrowingPoints.Add(firstGrowingPoint);

        }

        public AVQ()
        {
            originalImageValid = false;
            //Position firstGrowingPoint = new Position(0, 0);
            //poolGrowingPoints.Add(firstGrowingPoint);

            dictionary = new Dictionary<Block, int>();
            dictionaryBackup = new List<Block>();
            currentDictionaryLength = 256;
        }

        private static int SizeInBits(int value)
        {
            int size = 0;

            for (; value != 0; value >>= 1)
            {
                size++;
            }

            return size;
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
                int indexBitsNumber = SizeInBits(MaxDictionaryLength - 1);
                indexesList = new List<int>();

                for (int i = 0; i < indexesNumber; i++)
                {
                    int index = (int)reader.ReadNBits(indexBitsNumber);
                    indexesList.Add(index);

                }
            }
        }

        private static void WriteInFile(List<int> indexes)
        {
            int indexBitsNumber = SizeInBits(MaxDictionaryLength - 1);
            string[] fileNameParts = FormAVQ.InputFile.Split('.');
            string filenamesuffix = "_T" + Threshold + "_D" + MaxDictionaryLength;

            string filenameNoExtension = Path.GetFileNameWithoutExtension(FormAVQ.InputFile);
            string folderPath = Path.GetDirectoryName(FormAVQ.InputFile);

            string folder = Path.Combine(folderPath, filenameNoExtension);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string outputFile = Path.Combine(folder, filenameNoExtension + "_T" + Threshold + "_D" + MaxDictionaryLength + ".AVQ");


            using (BitWriter writer = new BitWriter(outputFile))
            {
                writer.WriteNBits((uint)indexes.Count, 32);
                writer.WriteNBits((uint)Threshold, 32);
                writer.WriteNBits((uint)MaxDictionaryLength, 32);
                writer.WriteNBits((uint)originalImage.Width, 32);
                writer.WriteNBits((uint)originalImage.Height, 32);

                foreach (uint index in indexes)
                {
                    writer.WriteNBits(index, indexBitsNumber);
                }

                writer.WriteNBits(9, 31);
            }


        }


        public static int CompareBlocks(Block b1, Block b2)
        {
            int differences = 0, contor = 0; ;

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

                    if (imageBitmap[b2.Position.X + i, b2.Position.Y + j] == false)
                    {
                        differences += Math.Abs(px1 - px2);
                        contor++;
                    }
                }
            }

            if (contor == 0)
            {
                return 0;
            }
            else
            {
                return differences / contor;
            }
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


        private void buildGP()
        {
            for (int i = 0; i < workImage.Width; i++)
            {
                for (int j = 0; j < workImage.Height; j++)
                {
                    poolGrowingPoints.Add(new Position(i, j));
                }
            }

            poolGrowingPoints = poolGrowingPoints.OrderBy(gp => gp.X + gp.Y).ToList();
        }

        private void UpdateGPPool()
        {

            for (int i = 0; i < poolGrowingPoints.Count; i++)
            {

                if (imageBitmap[poolGrowingPoints[i].X, poolGrowingPoints[i].Y] == true)
                {
                    poolGrowingPoints.RemoveAt(i);
                }
            }

            // return poolGrowingPoints[0];

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
            if (dictionaryChanged)
            {
                OdrerDictionary();
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
                    dictionaryBackup.Add(block);
                }
            }
        }

        private void OdrerDictionary()
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

        private void ReplaceBlock(Block findedBlock, int i, int j)
        {
            Color randomColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            for (int k = 0; k < findedBlock.Width; k++)
            {
                for (int l = 0; l < findedBlock.Height; l++)
                {

                    int color = workImage.GetPixel(findedBlock.Position.X + k, findedBlock.Position.Y + l);

                    if (i + k < workImage.Width && j + l < workImage.Height)
                    {

                        if (imageBitmap[findedBlock.Position.X + k, findedBlock.Position.Y + l] == false)
                        {
                            color = emptyPixel;
                            numberErrorPX++;
                        }

                        if (originalImageValid)
                        {
                            bitmapBlocks.SetPixel(i + k, j + l, randomColor);
                        }
                        workImage.SetPixel(i + k, j + l, color);
                        imageBitmap[i + k, j + l] = true;
                    }
                }
            }
        }


        public FastImage StartCompression(int th, int dictionarySize)
        {

            DateTime startTime = DateTime.Now;
            indexesList = new List<int>();
            Threshold = th;
            MaxDictionaryLength = dictionarySize;
            BlocPainFrequency = (int)1.0 * 100000 / MaxDictionaryLength;

            originalImage.Lock();
            workImage.Lock();

            buildGP();

            int ct = 0;
            while (poolGrowingPoints.Count != 0)
            {

                ct++;
                if (ct == BlocPainFrequency)
                {
                    ct = 0;
                    Program.form.updatePanelBlock(bitmapBlocks.GetBitMap());
                    //Thread.Sleep(50);
                }



                int widthStep, heightStep;
                //Position growingPoint = UpdateGPPool();
                Position growingPoint = poolGrowingPoints[0];
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


                }


                // TryAddGrowingPoint(growingPoint, widthStep, heightStep);
                //TryAddGrowingPoint(new Position(i + widthStep, j));

                if (i != 0 && j != 0)
                {
                    UpdateDictionary(growingPoint, index);
                }
                UpdateGPPool();
            }

            Program.form.updatePanelBlock(bitmapBlocks.GetBitMap());

            WriteInFile(indexesList);
            Console.WriteLine("NumberBlocksFinded = {0} numberErrorPX = {1}", numberBlocksFinded, numberErrorPX);

            workImage.Unlock();
            originalImage.Unlock();

            DateTime stopTime = DateTime.Now;
            Console.WriteLine(stopTime - startTime);
            return workImage;

        }

        public FastImage StartDeCompression(string inputFile)
        {
            ReadFromFile(inputFile);
           
            Bitmap blackImageBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            workImage = new FastImage(blackImageBitmap);
           
            workImage.Lock();
            buildGP();
            while (poolGrowingPoints.Count != 0)
            {
                int widthStep, heightStep;
                //Position growingPoint = UpdateGPPool();
                Position growingPoint = poolGrowingPoints[0];

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

                    ReplaceBlock(findedBlock, i, j);

                    widthStep = findedBlock.Width;
                    heightStep = findedBlock.Height;

                    numberBlocksFinded++;

                }

                //TryAddGrowingPoint(growingPoint, widthStep, heightStep);
                //TryAddGrowingPoint(new Position(i + widthStep, j));

                if (i != 0 && j != 0)
                {
                    UpdateDictionary(growingPoint, index);
                }

                UpdateGPPool();
            }

            Console.WriteLine("NumberBlocksFinded = {0} numberErrorPX = {1}", numberBlocksFinded, numberErrorPX);

            workImage.Unlock();

            return workImage;

        }


    }
}
