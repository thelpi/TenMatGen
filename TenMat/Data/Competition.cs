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

            SetDrawMatches(drawSize, availablePlayersRanked);
        }

        private void SetDrawMatches(int drawSize, IEnumerable<Player> availablePlayersRanked)
        {
            List<Player> listFixed = availablePlayersRanked.Take(DrawSize).ToList();

            var group1Seed = drawSize > 8 ? listFixed.Take(2).ToList() : new List<Player>();
            var group2Seed = drawSize > 16 ? listFixed.Except(group1Seed).Take(2).ToList() : new List<Player>();
            var group3Seed = drawSize > 32 ? listFixed.Except(group1Seed).Except(group2Seed).Take(4).ToList() : new List<Player>();
            var group4Seed = drawSize > 64 ? listFixed.Except(group1Seed).Except(group2Seed).Except(group3Seed).Take(8).ToList() : new List<Player>();
            var group0Seed = listFixed.Except(group1Seed).Except(group2Seed).Except(group3Seed).Except(group4Seed).ToList();

            List<Match>[] matchesBySeed = new List<Match>[4]
            {
                GenerateMatchesForSeededPlayers(group1Seed, group0Seed, false),
                GenerateMatchesForSeededPlayers(group2Seed, group0Seed, true),
                GenerateMatchesForSeededPlayers(group3Seed, group0Seed, true),
                GenerateMatchesForSeededPlayers(group4Seed, group0Seed, true)
            };

            FillDraw(matchesBySeed, GenerateUnseededMatches(group0Seed).ToList());
        }

        private IEnumerable<Match> GenerateUnseededMatches(List<Player> unseededPlayers)
        {
            var randomizedPlayersList = unseededPlayers
                .Where(p => p != null)
                .OrderBy(p => Tools.Rdm.Next())
                .ToList();

            for (int i = 1; i < randomizedPlayersList.Count; i += 2)
            {
                yield return GenerateMatchFromContext(randomizedPlayersList[i - 1],
                    randomizedPlayersList[i]);
            }
        }

        private Match GenerateMatchFromContext(Player p, Player p2)
        {
            return new Match(p, p2, Level.GetBestOf(), FifthSetTieBreakRule, Surface, Level, DrawSize.GetRound(), Date);
        }

        private void FillDraw(List<Match>[] matchesBySeed, List<Match> unseededMatches)
        {
            var matchesCountWithSeed = matchesBySeed.Sum(ms => ms.Count);
            var matchesUnseededCountBetweenTwoSeededMatch = unseededMatches.Count / matchesCountWithSeed;
            for (int i = 0; i < matchesCountWithSeed; i++)
            {
                int stackIndex = i % 8 == 0 ? 0 : (i % 4 == 0 ? 1 : (i % 2 == 0 ? 2 : 3));
                UnstackMatchIntoDraw(matchesBySeed[stackIndex]);
                for (int j = 0; j < matchesUnseededCountBetweenTwoSeededMatch; j++)
                {
                    _draw.Add(unseededMatches[(i * matchesUnseededCountBetweenTwoSeededMatch) + j]);
                }
            }
        }

        private void UnstackMatchIntoDraw(List<Match> seedMatches)
        {
            _draw.Add(seedMatches[0]);
            seedMatches.RemoveAt(0);
        }

        private List<Match> GenerateMatchesForSeededPlayers(List<Player> seededPlayers, List<Player> unseededPlayers, bool randomizeList)
        {
            var matches = new List<Match>();
            int seedsCount = seededPlayers.Count;
            for (int i = 0; i < seedsCount; i++ )
            {
                matches.Add(GenerateMatchFromContext(seededPlayers[i], UnstackRandomPlayer(unseededPlayers)));
            }

            return randomizeList ? matches.OrderBy(m => Tools.Rdm.Next()).ToList() : matches;
        }

        private static Player UnstackRandomPlayer(List<Player> players)
        {
            int randomIndex;
            do
            {
                randomIndex = Tools.Rdm.Next(0, players.Count);
            }
            while (players[randomIndex] == null);

            Player player = players[randomIndex];
            players[randomIndex] = null;
            return player;
        }
    }
}
