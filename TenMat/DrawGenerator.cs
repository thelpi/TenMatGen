using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat
{
    /// <summary>
    /// Draw generator.
    /// </summary>
    public class DrawGenerator
    {
        private readonly List<Tuple<uint, uint>> _draw;
        private readonly List<uint> _playerIdList;
        private readonly int _drawSize;
        private readonly double _seedRate;

        /// <summary>
        /// Draw; empty before the call of <see cref="GenerateDraw"/>.
        /// </summary>
        public IReadOnlyCollection<Tuple<uint, uint>> Draw { get { return _draw; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rankedPlayerIdList">Available players, sorted by ranking.</param>
        /// <param name="drawSize">Draw size.</param>
        /// <param name="seedRate">Seed rate.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="drawSize"/> should be a power of two between 8 and 128.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seedRate"/> should be between 0 and 1/2, and the multiplicative inverse of a power of two.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rankedPlayerIdList"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="rankedPlayerIdList"/> size should be greater or equal than the draw size.</exception>
        /// <exception cref="ArgumentException"><paramref name="rankedPlayerIdList"/> should not contain duplicates.</exception>
        public DrawGenerator(IEnumerable<uint> rankedPlayerIdList, int drawSize, double seedRate)
        {
            if (drawSize < 8 || drawSize > 128 || !drawSize.IsPowerOfTwo())
            {
                throw new ArgumentOutOfRangeException(nameof(drawSize), drawSize, "The draw should be a power of two between 8 and 128.");
            }

            if (seedRate != 0)
            {
                int seedRatePw = (int)Math.Floor(1 / seedRate);
                if (seedRatePw != 1 / seedRate || !seedRatePw.IsPowerOfTwo() || seedRate > 0.5 || seedRatePw >= drawSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(seedRate), seedRate, "The seed rate should be between 0 and 1/2, and the multiplicative inverse of a power of two.");
                }
            }

            if (rankedPlayerIdList == null)
            {
                throw new ArgumentNullException(nameof(rankedPlayerIdList));
            }

            if (rankedPlayerIdList.Count() < drawSize)
            {
                throw new ArgumentException("The players list size should be greater or equal than the draw size.", nameof(rankedPlayerIdList));
            }

            if (rankedPlayerIdList.Count() != rankedPlayerIdList.Distinct().Count())
            {
                throw new ArgumentException("The players list should not contain duplicates.", nameof(rankedPlayerIdList));
            }

            _draw = new List<Tuple<uint, uint>>();
            _playerIdList = rankedPlayerIdList.Take(drawSize).ToList();
            _drawSize = drawSize;
            _seedRate = seedRate;
        }

        /// <summary>
        /// Generates a draw.
        /// </summary>
        public void GenerateDraw()
        {
            List<List<uint>> seededPlayersBySeedValue = GetSeededPlayers();

            List<uint?> unseededPlayers = _playerIdList
                .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                .Select(p => (uint?)p)
                .ToList();

            var matchesBySeed = new List<List<Tuple<uint, uint>>>();
            for (int i = 0; i < seededPlayersBySeedValue.Count; i++)
            {
                matchesBySeed.Add(GenerateMatchesForSeededPlayers(seededPlayersBySeedValue[i], unseededPlayers, i > 0));
            }

            FillDraw(matchesBySeed, GenerateUnseededMatches(unseededPlayers).ToList());
        }

        private List<List<uint>> GetSeededPlayers()
        {
            Dictionary<int, int> newSeedsByDrawSize = new Dictionary<int, int>();
            int tmpDrawSize = _drawSize;
            while (tmpDrawSize >= 2 && _seedRate * tmpDrawSize > 1)
            {
                var seededCount = (int)(_seedRate * tmpDrawSize);
                newSeedsByDrawSize.Add(tmpDrawSize, seededCount == 2 ? 2 : seededCount / 2);
                tmpDrawSize /= 2;
            }

            var seededPlayersBySeedValue = new List<List<uint>>();
            foreach (int minDrawSize in newSeedsByDrawSize.Keys.Reverse())
            {
                var seededPlayers = new List<uint>();
                if (_drawSize >= minDrawSize)
                {
                    seededPlayers = _playerIdList
                        .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                        .Take(newSeedsByDrawSize[minDrawSize])
                        .ToList();
                }
                seededPlayersBySeedValue.Add(seededPlayers);
            }

            return seededPlayersBySeedValue;
        }

        private static IEnumerable<Tuple<uint, uint>> GenerateUnseededMatches(List<uint?> unseededPlayers)
        {
            var randomizedPlayersList = unseededPlayers
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .OrderBy(p => Tools.Rdm.Next())
                .ToList();

            for (int i = 1; i < randomizedPlayersList.Count; i += 2)
            {
                yield return new Tuple<uint, uint>(
                    randomizedPlayersList[i - 1],
                    randomizedPlayersList[i]);
            }
        }

        private void FillDraw(List<List<Tuple<uint, uint>>> matchesBySeed, List<Tuple<uint, uint>> unseededMatches)
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

        private void UnstackMatchIntoDraw(List<Tuple<uint, uint>> seedMatches)
        {
            _draw.Add(seedMatches[0]);
            seedMatches.RemoveAt(0);
        }

        private static List<Tuple<uint, uint>> GenerateMatchesForSeededPlayers(List<uint> seededPlayers, List<uint?> unseededPlayers, bool randomizeList)
        {
            var matches = new List<Tuple<uint, uint>>();
            int seedsCount = seededPlayers.Count;
            for (int i = 0; i < seedsCount; i++)
            {
                matches.Add(new Tuple<uint, uint>(
                    seededPlayers[i],
                    UnstackRandomPlayer(unseededPlayers)));
            }

            return randomizeList ? matches.OrderBy(m => Tools.Rdm.Next()).ToList() : matches;
        }

        private static uint UnstackRandomPlayer(List<uint?> players)
        {
            int randomIndex;
            do
            {
                randomIndex = Tools.Rdm.Next(0, players.Count);
            }
            while (!players[randomIndex].HasValue);

            uint player = players[randomIndex].Value;
            players[randomIndex] = null;
            return player;
        }
    }
}
