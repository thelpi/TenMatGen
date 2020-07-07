using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a competition.
    /// </summary>
    public class Competition
    {
        private readonly List<Match> _draw;

        /// <summary>
        /// Surface.
        /// </summary>
        public SurfaceEnum Surface { get; }
        /// <summary>
        /// Competition level.
        /// </summary>
        public LevelEnum Level { get; }
        /// <summary>
        /// Date.
        /// </summary>
        public DateTime Date { get; }
        /// <summary>
        /// Rule for fifth set tie-break.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drawGen">Instance of <see cref="DrawGenerator"/>.</param>
        /// <param name="date"><see cref="Date"/> value.</param>
        /// <param name="level"><see cref="Level"/> value.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        /// <param name="surface"><see cref="Surface"/> value.</param>
        /// <param name="availablePlayersRanked">List of available players, sorted by seed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="drawGen"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="availablePlayersRanked"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="availablePlayersRanked"/> should not contain duplicates.</exception>
        /// <exception cref="ArgumentException">The list of players should contains at least two elements.</exception>
        public Competition(DrawGenerator drawGen,
            DateTime date,
            LevelEnum level,
            FifthSetTieBreakRuleEnum fifthSetTieBreakRule,
            SurfaceEnum surface,
            IEnumerable<Player> availablePlayersRanked)
        {
            if (drawGen == null)
            {
                throw new ArgumentNullException(nameof(drawGen));
            }

            if (availablePlayersRanked == null)
            {
                throw new ArgumentNullException(nameof(availablePlayersRanked));
            }

            if (availablePlayersRanked.GroupBy(p => p.Id).Any(p => p.Count() > 1))
            {
                throw new ArgumentException("The list of players should not contain duplicates.", nameof(availablePlayersRanked));
            }

            if (availablePlayersRanked.Count() < 2)
            {
                throw new ArgumentException("The list of players should contains at least two elements.", nameof(availablePlayersRanked));
            }

            FifthSetTieBreakRule = fifthSetTieBreakRule;
            Date = date;
            Level = level;
            Surface = surface;
            _draw = drawGen
                .GenerateDraw(drawTuple =>
                    new Match(availablePlayersRanked.ElementAt(drawTuple.Item1),
                        availablePlayersRanked.ElementAt(drawTuple.Item2),
                        Level.GetBestOf(),
                        FifthSetTieBreakRule,
                        Surface,
                        Level,
                        drawGen.DrawSize.GetRound(),
                        Date))
                .ToList();
        }
    }
}
