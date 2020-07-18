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
        /// Winner player identifier; always index <c>0</c>.
        /// </summary>
        public uint WinnerId { get; }
        /// <summary>
        /// Loser player identifier; always index <c>1</c>.
        /// </summary>
        public uint LoserId { get; }
        /// <summary>
        /// Collection of sets.
        /// </summary>
        public IReadOnlyCollection<Set> Sets { get; }
        /// <summary>
        /// Service games count won by match winner, minus tie-breaks.
        /// </summary>
        public uint? WinnerSvGameCount { get; }
        /// <summary>
        /// Service games count won by match loser, minus tie-breaks.
        /// </summary>
        public uint? LoserSvGameCount { get; }

        /// <summary>
        /// Competition.
        /// </summary>
        public new CompetitionArchive Competition
        {
            get
            {
                return (CompetitionArchive)base.Competition;
            }
        }
        /// <summary>
        /// Games count, minus tie-breaks.
        /// </summary>
        public int GamesCount
        {
            get
            {
                return Sets.Sum(s => s.GamesCount) - Sets.Count(s => s.HasTieBreak);
            }
        }
        /// <summary>
        /// Tie-breaks won by match winner.
        /// </summary>
        public int WinnerTieBreakCount
        {
            get
            {
                return Sets.Count(s => s.HasTieBreak && s.IsWonBy(0));
            }
        }
        /// <summary>
        /// Tie-breaks won by match loser.
        /// </summary>
        public int LoserTieBreakCount
        {
            get
            {
                return Sets.Count(s => s.HasTieBreak && s.IsWonBy(1));
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="competition">The <see cref="MatchBase.Competition"/> value.</param>
        /// <param name="round">The <see cref="MatchBase.Round"/> value.</param>
        /// <param name="winnerId">The <see cref="WinnerId"/> value.</param>
        /// <param name="loserId">The <see cref="LoserId"/> value.</param>
        /// <param name="sets">Collection of set informations; respectively, for each set:
        /// <list type="bullet">
        /// <item>Number of games won by the winner of the match.</item>
        /// <item>Number of games won by the loser of the match.</item>
        /// <item>Optionnal number of points during the tie-break for the loser of the set.</item>
        /// </list>
        /// </param>
        /// <param name="winnerSvGameCount">The <see cref="WinnerSvGameCount"/> value.</param>
        /// <param name="loserSvGameCount">The <see cref="LoserSvGameCount"/> value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="competition"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sets"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sets"/> count should be lower than six.</exception>
        /// <exception cref="ArgumentException"><paramref name="sets"/> detail should not be null.</exception>
        public MatchArchive(CompetitionArchive competition, RoundEnum round,
            uint winnerId, uint loserId,
            IEnumerable<Tuple<uint, uint, uint?>> sets,
            uint? winnerSvGameCount, uint? loserSvGameCount)
            : base(competition, round)
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
            WinnerSvGameCount = winnerSvGameCount;
            LoserSvGameCount = loserSvGameCount;
        }
    }
}
