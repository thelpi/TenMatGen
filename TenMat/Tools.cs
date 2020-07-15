using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data.Enums;

namespace TenMat
{
    /// <summary>
    /// Static tools and extension methods.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// First year of the open era.
        /// </summary>
        public const int OPEN_ERA_YEAR = 1968;

        private static readonly Dictionary<int, RoundEnum> _roundByDrawSize = new Dictionary<int, RoundEnum>
        {
            { 65, RoundEnum.R128 },
            { 33, RoundEnum.R64 },
            { 17, RoundEnum.R32 },
            { 9, RoundEnum.R16 },
            { 5, RoundEnum.QF },
            { 3, RoundEnum.SF },
            { 2, RoundEnum.F }
        };

        /// <summary>
        /// Randomizer.
        /// </summary>
        public static Random Rdm { get; } = new Random();

        /// <summary>
        /// Simulates a coin flip.
        /// </summary>
        /// <returns><c>True</c> if heads; <c>False</c> if tails.</returns>
        public static bool FlipCoin()
        {
            return Rdm.Next(0, 2) == 1;
        }

        /// <summary>
        /// Checks if a specified number is a power of two.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns><c>True</c> if the number is a power of two.</returns>
        public static bool IsPowerOfTwo(this int number)
        {
            return number > 1 && Math.Pow(2, Math.Round(Math.Log(number, 2))) == number;
        }

        /// <summary>
        /// Gets the first <see cref="RoundEnum"/> value related to a draw size.
        /// </summary>
        /// <param name="drawSize">The draw size.</param>
        /// <returns><see cref="RoundEnum"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="drawSize"/> should be between 2 and 128.</exception>
        public static RoundEnum GetFirstRound(int drawSize)
        {
            if (drawSize > 128 || drawSize < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(drawSize), drawSize, "The draw size should be between 2 and 128.");
            }

            return _roundByDrawSize
                .OrderByDescending(kvp => kvp.Key)
                .First(kvp => kvp.Key <= drawSize)
                .Value;
        }

        /// <summary>
        /// Gets a random <see cref="DateTime"/> between the specified range.
        /// </summary>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        /// <returns>Random <see cref="DateTime"/>.</returns>
        public static DateTime GetRandomDate(this DateTime start, DateTime end)
        {
            if (start.Date == end.Date)
            {
                return start.Date;
            }

            if (start.Date > end.Date)
            {
                DateTime tmp = end;
                end = start;
                start = tmp;
            }

            var daysMax = (int)((end.Date - start.Date).TotalDays);

            return start.Date.AddDays(Rdm.Next(daysMax));
        }
    }
}
