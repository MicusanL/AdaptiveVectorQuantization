
using System.Drawing;
using System.Drawing.Imaging;

namespace AdaptiveVectorQuantization
{
    public class FastImage
    {
        public int Height = 0;
        public int Width = 0;
        private Bitmap image = null;
        private Rectangle rectangle;
        private BitmapData bitmapData = null;
        private int color;
        private int currentBitmapWidth = 0;

        struct PixelData
        {
            public byte red, green, blue;
        }

        public FastImage(Bitmap bitmap)
        {
            image = bitmap;
            Width = image.Width;
            Height = image.Height;
            //size = new Point(image.Size);
            currentBitmapWidth = bitmap.Width;
        }

        public void Lock()
        {
            // Rectangle For Locking The Bitmap In Memory
            rectangle = new Rectangle(0, 0, image.Width, image.Height);
            // Get The Bitmap's Pixel Data From The Locked Bitmap
            bitmapData = image.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        }

        public void Unlock()
        {
            image.UnlockBits(bitmapData);
        }

        public int GetPixel(int col, int row)
        {
           
            unsafe
            {
                PixelData* pBase = (PixelData*)bitmapData.Scan0;
                PixelData* pPixel = pBase + row * currentBitmapWidth + col;
                color = (int)(pPixel->red + pPixel->green + pPixel->blue) / 3;
            }
            return color;
        }

        public void SetPixel(int col, int row, int r, int g, int b)
        {
            unsafe
            {
                PixelData* pBase = (PixelData*)bitmapData.Scan0;
                PixelData* pPixel = pBase + row * currentBitmapWidth + col;
                pPixel->red = (byte)r;
                pPixel->green = (byte)g;
                pPixel->blue = (byte)b;
            }
        }

        public void SetPixel(int col, int row, int color)
        {
            unsafe
            {
                PixelData* pBase = (PixelData*)bitmapData.Scan0;
                PixelData* pPixel = pBase + row * currentBitmapWidth + col;
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
