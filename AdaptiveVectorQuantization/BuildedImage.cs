namespace AdaptiveVectorQuantization
{
    public static class BuildedImage
    {
        public static int Height { get; set; }
        public static int Width { get; set; }
        public static int[,] CompressedImage { get; set; } = new int[Height, Width];
    }
}
