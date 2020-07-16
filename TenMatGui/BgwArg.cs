using System;
using System.Collections.Generic;
using TenMat.Data;
using TenMat.Data.Enums;

namespace TenMatGui
{
    internal class BgwArg
    {
        public int DrawSize { get; set; }
        public uint SeedRate { get; set; }
        public DateTime StartDate { get; set; }
        public LevelEnum Level { get; set; }
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; set; }
        public SurfaceEnum Surface { get; set; }
        public IReadOnlyCollection<Player> Players { get; set; }
        public BestOfEnum BestOf { get; set; }
        public BestOfEnum FinalBestOf { get; set; }
    }
}
