using System.Security.Cryptography;

namespace MazeWalking.Web.Utils
{
    public static class RandomUtil

    {
        public static double RandomDouble()
        {
            Span<byte> b = stackalloc byte[8];
            RandomNumberGenerator.Fill(b);
            ulong val = BitConverter.ToUInt64(b);
            return val / (double)ulong.MaxValue;
        }

    }
}
