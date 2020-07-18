using System;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Competition base class.
    /// </summary>
    public abstract class CompetitionBase
    {
        /// <summary>
        /// Surface.
        /// </summary>
        public SurfaceEnum Surface { get; }
        /// <summary>
        /// Is indoor y/n.
        /// </summary>
        public bool IsIndoor { get; }
        /// <summary>
        /// Competition level.
        /// </summary>
        public LevelEnum Level { get; }
        /// <summary>
        /// Date.
        /// </summary>
        public DateTime Date { get; }
        /// <summary>
        /// Sets best-of for matches except final.
        /// </summary>
        public BestOfEnum BestOf { get; private set; } = BestOfEnum.Three;
        /// <summary>
        /// Sets best-of for final match.
        /// </summary>
        public BestOfEnum FinalBestOf { get; private set; } = BestOfEnum.Three;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="surface">The <see cref="Surface"/> value.</param>
        /// <param name="isIndoor">The <see cref="IsIndoor"/> value.</param>
        /// <param name="level">The <see cref="Level"/> value.</param>
        /// <param name="date">The <see cref="Date"/> value.</param>
        protected CompetitionBase(SurfaceEnum surface, bool isIndoor, LevelEnum level, DateTime date)
        {
            Surface = surface;
            IsIndoor = isIndoor;
            Level = level;
            Date = date;
        }

        /// <summary>
        /// Gets the <see cref="BestOfEnum"/> value for the specified round.
        /// </summary>
        /// <param name="round"><see cref="RoundEnum"/> value.</param>
        /// <returns><see cref="BestOfEnum"/> value.</returns>
        public BestOfEnum GetBestOf(RoundEnum round)
        {
            return round == RoundEnum.F ? FinalBestOf : BestOf;
        }

        /// <summary>
        /// Sets <see cref="FinalBestOf"/> or <see cref="BestOf"/> to <see cref="BestOfEnum.Five"/>.
        /// </summary>
        /// <param name="isFinal"><c>True</c> to set <see cref="FinalBestOf"/>.</param>
        protected void SetBestOfFive(bool isFinal)
        {
            if (isFinal)
            {
                FinalBestOf = BestOfEnum.Five;
            }
            else
            {
                BestOf = BestOfEnum.Five;
            }
        }
    }
}
