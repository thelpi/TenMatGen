using System;
using System.Collections.Generic;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents an archived competition.
    /// </summary>
    /// <seealso cref="CompetitionBase"/>
    public class CompetitionArchive : CompetitionBase
    {
        private static Dictionary<uint, CompetitionArchive> _competitions
            = new Dictionary<uint, CompetitionArchive>();

        private CompetitionArchive(SurfaceEnum surface, bool isIndoor, LevelEnum level, DateTime date)
            : base(surface, isIndoor, level, date) { }

        /// <summary>
        /// Creates or retrieves a <see cref="CompetitionArchive"/> instance from database informations about a single match.
        /// </summary>
        /// <param name="id">Competition identifier.</param>
        /// <param name="surface">The <see cref="CompetitionBase.Surface"/> value.</param>
        /// <param name="isIndoor">The <see cref="CompetitionBase.IsIndoor"/> value.</param>
        /// <param name="level">The <see cref="CompetitionBase.Level"/> value.</param>
        /// <param name="date">The <see cref="CompetitionBase.Date"/> value.</param>
        /// <param name="bestOf">The <see cref="BestOfEnum"/> value from the source match.</param>
        /// <param name="isFinal"><c>True</c> if the source match is the final.</param>
        /// <returns>Instance of <see cref="CompetitionArchive"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> should be a number greater than 0.</exception>
        public static CompetitionArchive CreateNewOrGet(uint id, SurfaceEnum surface,
            bool isIndoor, LevelEnum level, DateTime date, BestOfEnum bestOf, bool isFinal)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Identifier should be a number greater than 0.");
            }

            if (!_competitions.ContainsKey(id))
            {
                _competitions.Add(id, new CompetitionArchive(surface, isIndoor, level, date));
            }

            CompetitionArchive competition = _competitions[id];

            if (bestOf == BestOfEnum.Five)
            {
                competition.SetBestOfFive(isFinal);
            }

            return competition;
        }
    }
}
