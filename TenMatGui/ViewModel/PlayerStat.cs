using System;
using TenMat.Data;

namespace TenMatGui.ViewModel
{
    internal class PlayerStat : IComparable<PlayerStat>
    {
        public const int MaxWidth = 1000;

        public static int TotalCpt { get; private set; }

        public int WinCpt { get; private set; }
        public string PlayerName { get; }
        public uint Id { get; }
        public int StatWidth { get; private set; }
        public double PlayerPercent { get; private set; }

        public PlayerStat(Player p)
        {
            Id = p.Id;
            PlayerName = p.Name;
            WinCpt = 0;
            StatWidth = 0;
            PlayerPercent = 0;
        }

        public void AddCpt()
        {
            TotalCpt++;
            WinCpt++;
        }

        public void RefreshCpt()
        {
            StatWidth = (int)(MaxWidth * WinCpt / (double)TotalCpt);
            PlayerPercent = Math.Round((WinCpt  / (double)TotalCpt) * 100, 3);
        }

        public static void ResetCpt()
        {
            TotalCpt = 0;
        }

        int IComparable<PlayerStat>.CompareTo(PlayerStat other)
        {
            return WinCpt < other.WinCpt ? 1 : (other.WinCpt < WinCpt ? -1 : 0);
        }
    }
}
