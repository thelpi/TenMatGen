using System;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    public class MatchHistory
    {
        public SurfaceEnum Surface { get; set; }
        public LevelEnum Level { get; set; }
        public RoundEnum Round { get; set; }
        public uint WinnerId { get; set; }
        public uint LoserId { get; set; }
        public uint BestOf { get; set; }
        public DateTime Date { get; set; }
    }
}
