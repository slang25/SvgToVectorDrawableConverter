namespace PathFillTypeConverter
{
    internal static class ConvertQuality
    {
        public static double PiecewiseLinearApproximatorMaxPieceLength;

        public static void SetDefault()
        {
            PiecewiseLinearApproximatorMaxPieceLength = 0.05;
        }

        public static void Degrade()
        {
            PiecewiseLinearApproximatorMaxPieceLength *= 10;
        }
    }
}
