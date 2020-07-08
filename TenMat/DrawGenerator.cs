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
        /// <summary>
        /// Seed rate.
        /// </summary>
        public double SeedRate { get; }
        /// <summary>
        /// Draw size.
        /// </summary>
        public int DrawSize { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drawSize">Draw size.</param>
        /// <param name="seedRate">Seed rate.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="drawSize"/> should be a power of two between 8 and 128.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seedRate"/> should be between 0 and 1/2, and the multiplicative inverse of a power of two.</exception>
        public DrawGenerator(int drawSize, double seedRate)
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
            
            DrawSize = drawSize;
            SeedRate = seedRate;
        }

        /// <summary>
        /// Generates a draw.
        /// </summary>
        /// <typeparam name="TMatch">Match target type.</typeparam>
        /// <param name="toMatch">Delegate to transform a double-index tuple into an instance of <typeparamref name="TMatch"/>.</param>
        /// <returns>A list of <see cref="TMatch"/>.</returns>
        public List<TMatch> GenerateDraw<TMatch>(Func<Tuple<int, int>, TMatch> toMatch)
        {
            List<int> indexList = Enumerable.Range(0, DrawSize).ToList();

            List<List<int>> seededPlayersBySeedValue = GetSeededPlayers(DrawSize, SeedRate, indexList);

            List<int?> unseededPlayers = indexList
                .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                .Select(p => (int?)p)
                .ToList();

            var matchesBySeed = new List<List<Tuple<int, int>>>();
            for (int i = 0; i < seededPlayersBySeedValue.Count; i++)
            {
                matchesBySeed.Add(GenerateMatchesForSeededPlayers(seededPlayersBySeedValue[i], unseededPlayers, i > 0));
            }

            IEnumerable<Tuple<int, int>> matchesIndex = FillDraw(matchesBySeed, GenerateUnseededMatches(unseededPlayers).ToList());

            List<Tuple<int, int>> matchesIndexFixed = matchesIndex.ToList();
            var matches = new List<TMatch>();
            
            int halfCount = 1;
            for (int i = 0; i < matchesIndexFixed.Count; i++)
            {
                if (i >= matchesIndexFixed.Count / 2)
                {
                    matches.Add(toMatch(matchesIndexFixed.ElementAt(matchesIndexFixed.Count - halfCount)));
                    halfCount++;
                }
                else
                {
                    matches.Add(toMatch(matchesIndexFixed.ElementAt(i)));
                }
            }

            return matches;
        }

        private static List<List<int>> GetSeededPlayers(int drawSize, double seedRate, List<int> indexList)
        {
            Dictionary<int, int> newSeedsByDrawSize = new Dictionary<int, int>();
            int tmpDrawSize = drawSize;
            while (tmpDrawSize >= 2 && seedRate * tmpDrawSize > 1)
            {
                var seededCount = (int)(seedRate * tmpDrawSize);
                newSeedsByDrawSize.Add(tmpDrawSize, seededCount == 2 ? 2 : seededCount / 2);
                tmpDrawSize /= 2;
            }

            var seededPlayersBySeedValue = new List<List<int>>();
            foreach (int minDrawSize in newSeedsByDrawSize.Keys.Reverse())
            {
                var seededPlayers = new List<int>();
                if (drawSize >= minDrawSize)
                {
                    seededPlayers = indexList
                        .Except(seededPlayersBySeedValue.SelectMany(sp => sp))
                        .Take(newSeedsByDrawSize[minDrawSize])
                        .ToList();
                }
                seededPlayersBySeedValue.Add(seededPlayers);
            }

            return seededPlayersBySeedValue;
        }

        private static IEnumerable<Tuple<int, int>> GenerateUnseededMatches(List<int?> unseededPlayers)
        {
            var randomizedPlayersList = unseededPlayers
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .OrderBy(p => Tools.Rdm.Next())
                .ToList();

            for (int i = 1; i < randomizedPlayersList.Count; i += 2)
            {
                yield return new Tuple<int, int>(
                    randomizedPlayersList[i - 1],
                    randomizedPlayersList[i]);
            }
        }

        private static IEnumerable<Tuple<int, int>> FillDraw(List<List<Tuple<int, int>>> matchesBySeed, List<Tuple<int, int>> unseededMatches)
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

                yield return matchesBySeed[(matchesBySeed.Count - 1) - stackIndex][0];
                matchesBySeed[(matchesBySeed.Count - 1) - stackIndex].RemoveAt(0);

                for (int j = 0; j < matchesUnseededCountBetweenTwoSeededMatch; j++)
                {
                    yield return unseededMatches[(i * matchesUnseededCountBetweenTwoSeededMatch) + j];
                }
            }
        }

        private static List<Tuple<int, int>> GenerateMatchesForSeededPlayers(List<int> seededPlayers, List<int?> unseededPlayers, bool randomizeList)
        {
            var matches = new List<Tuple<int, int>>();
            int seedsCount = seededPlayers.Count;
            for (int i = 0; i < seedsCount; i++)
            {
                matches.Add(new Tuple<int, int>(
                    seededPlayers[i],
                    UnstackRandomPlayer(unseededPlayers)));
            }

            return randomizeList ? matches.OrderBy(m => Tools.Rdm.Next()).ToList() : matches;
        }

        private static int UnstackRandomPlayer(List<int?> players)
        {
            int randomIndex;
            do
            {
                randomIndex = Tools.Rdm.Next(0, players.Count);
            }
            while (!players[randomIndex].HasValue);

            int player = players[randomIndex].Value;
            players[randomIndex] = null;
            return player;
        }
    }
}
