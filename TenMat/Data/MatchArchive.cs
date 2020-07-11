using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Collection of sets.
        /// </summary>
        public IReadOnlyCollection<Set> Sets { get; }

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
        /// <param name="sets">Collection of set informations; respectively, for each set:
        /// <list type="bullet">
        /// <item>Number of games won by the winner of the match.</item>
        /// <item>Number of games won by the loser of the match.</item>
        /// <item>Optionnal number of points during the tie-break for the loser of the set.</item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="sets"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sets"/> count should be lower than six.</exception>
        /// <exception cref="ArgumentException"><paramref name="sets"/> detail should not be null.</exception>
        public MatchArchive(SurfaceEnum surface, LevelEnum level, RoundEnum round,
            BestOfEnum bestOf, DateTime tournamentBeginningDate,
            uint winnerId, uint loserId, IEnumerable<Tuple<uint, uint, uint?>> sets)
            : base(surface, level, round, bestOf, tournamentBeginningDate)
        {
            if (sets == null)
            {
                throw new ArgumentNullException(nameof(sets));
            }

            if (sets.Count() > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(sets), sets.Count(), "Sets count should be lower than six.");
            }

            if (sets.Any(s => s == null))
            {
                throw new ArgumentException("Set detail should not be null.", nameof(sets));
            }

            WinnerId = winnerId;
            LoserId = loserId;
            Sets = sets.Select(s => Set.CreateFromArchive(s.Item1, s.Item2, s.Item3)).ToList();
        }
    }
}
