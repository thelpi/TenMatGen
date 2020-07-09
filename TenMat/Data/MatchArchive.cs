using System;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a match archive.
    /// </summary>
    /// <seealso cref="MatchBase"/>
    public class MatchArchive : MatchBase
    {
        /// <summary>
        /// Winner player identifier.
        /// </summary>
        public uint WinnerId { get; }
        /// <summary>
        /// Loser player identifier.
        /// </summary>
        public uint LoserId { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="surface">The <see cref="MatchBase.Surface"/> value.</param>
        /// <param name="level">The <see cref="MatchBase.Level"/> value.</param>
        /// <param name="round">The <see cref="MatchBase.Round"/> value.</param>
        /// <param name="bestOf">The <see cref="MatchBase.BestOf"/> value.</param>
        /// <param name="tournamentBeginningDate">The <see cref="MatchBase.TournamentBeginningDate"/> value.</param>
        /// <param name="winnerId">The <see cref="WinnerId"/> value.</param>
        /// <param name="loserId">The <see cref="LoserId"/> value.</param>
        public MatchArchive(SurfaceEnum surface, LevelEnum level, RoundEnum round,
            BestOfEnum bestOf, DateTime tournamentBeginningDate,
            uint winnerId, uint loserId)
            : base(surface, level, round, bestOf, tournamentBeginningDate)
        {
            WinnerId = winnerId;
            LoserId = loserId;
        }
    }
}
