using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a competition.
    /// </summary>
    public class Competition
    {
        private readonly List<Match> _draw;

        /// <summary>
        /// Surface.
        /// </summary>
        public SurfaceEnum Surface { get; }
        /// <summary>
        /// Competition level.
        /// </summary>
        public LevelEnum Level { get; }
        /// <summary>
        /// Date.
        /// </summary>
        public DateTime Date { get; }
        /// <summary>
        /// Draw size.
        /// </summary>
        public int DrawSize { get; }
        /// <summary>
        /// Rule for fifth set tie-break.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drawSize"><see cref="DrawSize"/> value.</param>
        /// <param name="date"><see cref="Date"/> value.</param>
        /// <param name="level"><see cref="Level"/> value.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        /// <param name="surface"><see cref="Surface"/> value.</param>
        /// <param name="availablePlayersRanked">List of available players, sorted by seed.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="drawSize"/> be a power of two between 8 and 128.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="availablePlayersRanked"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">The players list size should be greater or equal than the draw size.</exception>
        /// <exception cref="ArgumentException">The players list should not contain duplicates.</exception>
        public Competition(int drawSize, DateTime date, LevelEnum level, FifthSetTieBreakRuleEnum fifthSetTieBreakRule,
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
            List<Player> drawPlayersList = availablePlayersRanked.Take(DrawSize).ToList();

            List<List<Player>> seededPlayersBySeedValue = GetSeededPlayers(drawSize, drawPlayersList);

            List<Player> unseededPlayers = drawPlayersList
                .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                .ToList();

            List<List<Match>> matchesBySeed = new List<List<Match>>();
            for (int i = 0; i < seededPlayersBySeedValue.Count; i++)
            {
                matchesBySeed.Add(GenerateMatchesForSeededPlayers(seededPlayersBySeedValue[i], unseededPlayers, i > 0));
            }

            FillDraw(matchesBySeed, GenerateUnseededMatches(unseededPlayers).ToList());
        }

        private static List<List<Player>> GetSeededPlayers(int drawSize, List<Player> drawPlayersList)
        {
            // THIS TO REMOVE AND THE SYSTEM IS GENERIC
            Dictionary<int, int> newSeedsByDrawSize = new Dictionary<int, int>
            {
                { 8, 2 },
                { 16, 2 },
                { 32, 4 },
                { 64, 8 }
            };

            List<List<Player>> seededPlayersBySeedValue = new List<List<Player>>();
            foreach (int minDrawSize in newSeedsByDrawSize.Keys)
            {
                var seededPlayers = new List<Player>();
                if (drawSize >= minDrawSize)
                {
                    seededPlayers = drawPlayersList
                        .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                        .Take(newSeedsByDrawSize[minDrawSize])
                        .ToList();
                }
                seededPlayersBySeedValue.Add(seededPlayers);
            }

            return seededPlayersBySeedValue;
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

        private void FillDraw(List<List<Match>> matchesBySeed, List<Match> unseededMatches)
        {
            var stackIndexByJumpSize = new Dictionary<int, int>();
            for (int i = 1; i < matchesBySeed.Count; i++)
            {
                stackIndexByJumpSize.Add((int)Math.Pow(2, i), i);
            }
            stackIndexByJumpSize = stackIndexByJumpSize
                .Reverse()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var matchesCountWithSeed = matchesBySeed.Sum(ms => ms.Count);
            var matchesUnseededCountBetweenTwoSeededMatch = unseededMatches.Count / matchesCountWithSeed;
            for (int i = 0; i < matchesCountWithSeed; i++)
            {
                int stackIndex = 0;
                foreach (int jumpSize in stackIndexByJumpSize.Keys)
                {
                    if (i % jumpSize == 0)
                    {
                        stackIndex = stackIndexByJumpSize[jumpSize];
                        break;
                    }
                }
                UnstackMatchIntoDraw(matchesBySeed[(matchesBySeed.Count - 1) - stackIndex]);
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
