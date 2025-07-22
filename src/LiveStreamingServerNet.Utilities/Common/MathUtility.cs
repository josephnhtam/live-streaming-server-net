namespace LiveStreamingServerNet.Utilities.Common
{
    public static class MathUtility
    {
        public static int NextPowerOfTwo(int n)
        {
            if (n <= 1)
                return 1;

            if (n > 0x40000000)
                throw new ArgumentOutOfRangeException();

            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
    }
}
