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
        /// Draw size.
        /// </summary>
        public int DrawSize { get; }
        /// <summary>
        /// Rule for fifth set tie-break.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drawSize"><see cref="DrawSize"/> value.</param>
        /// <param name="date"><see cref="Date"/> value.</param>
        /// <param name="level"><see cref="Level"/> value.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        /// <param name="surface"><see cref="Surface"/> value.</param>
        /// <param name="availablePlayersRanked">List of available players, sorted by seed.</param>
        /// <param name="seedRate">Rate of seeded players compared to <see cref="drawSize"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="availablePlayersRanked"/> is <c>Null</c>.</exception>
        public Competition(int drawSize, DateTime date, LevelEnum level, FifthSetTieBreakRuleEnum fifthSetTieBreakRule,
            SurfaceEnum surface, IEnumerable<Player> availablePlayersRanked, double seedRate)
        {
            if (availablePlayersRanked == null)
            {
                throw new ArgumentNullException(nameof(availablePlayersRanked));
            }

            FifthSetTieBreakRule = fifthSetTieBreakRule;
            DrawSize = drawSize;
            Date = date;
            Level = level;
            Surface = surface;
            DrawGenerator dg = new DrawGenerator(availablePlayersRanked.Select(p => p.Id), drawSize, seedRate);
            dg.GenerateDraw();
            _draw = dg.Draw
                .Select(d =>
                    new Match(availablePlayersRanked.Single(p => p.Id == d.Item1),
                        availablePlayersRanked.Single(p => p.Id == d.Item2),
                        Level.GetBestOf(),
                        FifthSetTieBreakRule,
                        Surface,
                        Level,
                        DrawSize.GetRound(),
                        Date)
                    )
                .ToList();
        }
    }
}
