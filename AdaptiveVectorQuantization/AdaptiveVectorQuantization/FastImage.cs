
using System.Drawing;
using System.Drawing.Imaging;

namespace AdaptiveVectorQuantization
{
    public class FastImage
    {
        public int Height = 0;
        public int Width = 0;

        private readonly Bitmap image = null;
        private Rectangle rectangle;
        private BitmapData bitmapData = null;

        private unsafe PixelData* pBase;

        private readonly int[,] pixelMatrix;

        private struct PixelData
        {
            public byte red, green, blue;
        }


        public FastImage(Bitmap bitmap)
        {
            image = bitmap;
            Width = image.Width;
            Height = image.Height;
            rectangle = new Rectangle(0, 0, image.Width, image.Height);

            pixelMatrix = new int[Width, Height];

            Lock();

            unsafe
            {
                pBase = (PixelData*)bitmapData.Scan0;

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        PixelData* pPixel = pBase + i * Width + j;
                        pixelMatrix[i, j] = (pPixel->red + pPixel->green + pPixel->blue) / 3;
                    }
                }
            }

            Unlock();
        }

        public void Lock()
        {
            bitmapData = image.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        }

        public void Unlock()
        {
            image.UnlockBits(bitmapData);
        }

        public int GetPixel(int col, int row)
        {
            return pixelMatrix[row, col];
        }

        public void SetPixel(int col, int row, Color color)
        {
            unsafe
            {
                PixelData* pPixel = pBase + row * Width + col;
                pPixel->red = color.R;
                pPixel->green = color.G;
                pPixel->blue = color.B;

            }
        }

        public void SetPixel(int col, int row, int color)
        {
            pixelMatrix[row, col] = color;

            unsafe
            {
                PixelData* pPixel = pBase + row * Width + col;
                pPixel->red = (byte)color;
                pPixel->green = (byte)color;
                pPixel->blue = (byte)color;
            }

        }

        public Bitmap GetBitMap()
        {
            return image;
        }

    }
}
