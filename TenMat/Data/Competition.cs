using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    public class Competition
    {
        private readonly List<Match> _draw;

        public SurfaceEnum Surface { get; }
        public LevelEnum Level { get; }
        public DateTime Date { get; }
        public int DrawSize { get; }
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }

        public Competition(int drawSize, DateTime date, LevelEnum level,
            FifthSetTieBreakRuleEnum fifthSetTieBreakRule,
            SurfaceEnum surface, IEnumerable<Player> availablePlayersRanked)
        {
            if (drawSize < 8 || drawSize > 128 || !drawSize.IsPowerOfTwo())
            {
                throw new ArgumentOutOfRangeException(nameof(drawSize), drawSize, "The draw should be a power of two between 8 and 128.");
            }

            if (availablePlayersRanked == null)
            {
                throw new ArgumentNullException(nameof(availablePlayersRanked));
            }

            if (availablePlayersRanked.Count() < DrawSize)
            {
                throw new ArgumentException("The players list size should be greater or equal than the draw size.", nameof(availablePlayersRanked));
            }

            if (availablePlayersRanked.GroupBy(p => p.Id).Any(pGroup => pGroup.Count() > 1))
            {
                throw new ArgumentException("The players list should not contain duplicates.", nameof(availablePlayersRanked));
            }

            FifthSetTieBreakRule = fifthSetTieBreakRule;
            DrawSize = drawSize;
            Date = date;
            Level = level;
            Surface = surface;
            _draw = new List<Match>();

            List<Player> listFixed = availablePlayersRanked.Take(DrawSize).ToList();

            var group1Seed = drawSize > 8 ? listFixed.Take(2).ToList() : new List<Player>();
            var group2Seed = drawSize > 16 ? listFixed.Except(group1Seed).Take(2).ToList() : new List<Player>();
            var group3Seed = drawSize > 32 ? listFixed.Except(group1Seed).Except(group2Seed).Take(4).ToList() : new List<Player>();
            var group4Seed = drawSize > 64 ? listFixed.Except(group1Seed).Except(group2Seed).Except(group3Seed).Take(8).ToList() : new List<Player>();
            var group0Seed = listFixed.Except(group1Seed).Except(group2Seed).Except(group3Seed).Except(group4Seed).ToList();

            var matchSeed1 = DrawMatchesForList(group1Seed, group0Seed, false);
            var matchSeed2 = DrawMatchesForList(group2Seed, group0Seed);
            var matchSeed3 = DrawMatchesForList(group3Seed, group0Seed);
            var matchSeed4 = DrawMatchesForList(group4Seed, group0Seed);

            group0Seed = group0Seed.Where(p => p != null).OrderBy(p => Tools.Rdm.Next()).ToList();
            var matchSeed0 = group0Seed
                                .Take(group0Seed.Count / 2)
                                .Select(p => new Match(p, group0Seed.Skip(group0Seed.Count / 2).ElementAt(group0Seed.IndexOf(p)), Level.GetBestOf(), FifthSetTieBreakRule, Surface, Level, DrawSize.GetRound(), Date))
                                .ToList();

            var seedPlaceholderCount = matchSeed1.Count + matchSeed2.Count + matchSeed3.Count + matchSeed4.Count;
            var jumpsCount = matchSeed0.Count / seedPlaceholderCount;
            for (int i = 0; i < seedPlaceholderCount; i++)
            {
                if (i % 8 == 0)
                {
                    _draw.Add(matchSeed1[0]);
                    matchSeed1.RemoveAt(0);
                }
                else if (i % 4 == 0)
                {
                    _draw.Add(matchSeed2[0]);
                    matchSeed2.RemoveAt(0);
                }
                else if (i % 2 == 0)
                {
                    _draw.Add(matchSeed3[0]);
                    matchSeed3.RemoveAt(0);
                }
                else
                {
                    _draw.Add(matchSeed4[0]);
                    matchSeed4.RemoveAt(0);
                }
                for (int j = 0; j < jumpsCount; j++)
                {
                    _draw.Add(matchSeed0[(i * jumpsCount) + j]);
                }
            }
        }

        private List<Match> DrawMatchesForList(List<Player> seeds, List<Player> unseeds, bool randomize = true)
        {
            var matches = new List<Match>();
            int seedsCount = seeds.Count;
            for (int i = 0; i < seedsCount; i++ )
            {
                if (seeds[i] != null)
                {
                    matches.Add(new Match(seeds[i], GetRandomPlayer(unseeds, i), Level.GetBestOf(), FifthSetTieBreakRule, Surface, Level, DrawSize.GetRound(), Date));
                    seeds[i] = null;
                }
            }

            if (!randomize)
            {
                return matches;
            }

            return matches.OrderBy(m => Tools.Rdm.Next()).ToList();
        }

        private static Player GetRandomPlayer(List<Player> players, int? i = null)
        {
            int randomIndex;
            do
            {
                randomIndex = Tools.Rdm.Next(0, players.Count);
            }
            while (players[randomIndex] == null || randomIndex == i);

            Player player = players[randomIndex];
            players[randomIndex] = null;
            return player;
        }
    }
}
