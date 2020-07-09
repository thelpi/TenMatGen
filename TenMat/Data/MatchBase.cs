using System;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Match base class.
    /// </summary>
    public abstract class MatchBase
    {
        /// <summary>
        /// Surface.
        /// </summary>
        public SurfaceEnum Surface { get; }

        /// <summary>
        /// Competition level.
        /// </summary>
        public LevelEnum Level { get; }

        /// <summary>
        /// Round.
        /// </summary>
        public RoundEnum Round { get; }

        /// <summary>
        /// Sets best-of.
        /// </summary>
        public BestOfEnum BestOf { get; }

        /// <summary>
        /// Tournament beginning date.
        /// </summary>
        public DateTime TournamentBeginningDate { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="surface">The <see cref="Surface"/> value.</param>
        /// <param name="level">The <see cref="Level"/> value.</param>
        /// <param name="round">The <see cref="Round"/> value.</param>
        /// <param name="bestOf">The <see cref="BestOf"/> value.</param>
        /// <param name="tournamentBeginningDate">The <see cref="TournamentBeginningDate"/> value.</param>
        protected MatchBase(SurfaceEnum surface, LevelEnum level, RoundEnum round,
            BestOfEnum bestOf, DateTime tournamentBeginningDate)
        {
            Surface = surface;
            Level = level;
            Round = round;
            BestOf = bestOf;
            TournamentBeginningDate = tournamentBeginningDate;
        }
    }
}
