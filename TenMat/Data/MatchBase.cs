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
        /// <see cref="Competition"/> instance.
        /// </summary>
        protected CompetitionBase Competition { get; }

        /// <summary>
        /// Round.
        /// </summary>
        public RoundEnum Round { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="competition">The <see cref="Competition"/> value.</param>
        /// <param name="round">The <see cref="Round"/> value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="competition"/> is <c>Null</c>.</exception>
        protected MatchBase(CompetitionBase competition, RoundEnum round)
        {
            Competition = competition ?? throw new ArgumentNullException(nameof(competition));
            Round = round;
        }
    }
}
