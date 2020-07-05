using System;

namespace TenMat
{
    public static class Tools
    {
        public static Random Rdm { get; } = new Random();

        public static bool CoinDraw()
        {
            return Rdm.Next(0, 2) == 1;
        }
    }
}
