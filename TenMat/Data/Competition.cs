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
        private readonly Dictionary<RoundEnum, IReadOnlyList<Match>> _draw;

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
        /// Draw by round.
        /// </summary>
        public IReadOnlyDictionary<RoundEnum, IReadOnlyList<Match>> Draw
        {
            get
            {
                return _draw;
            }
        }

        /// <summary>
        /// Indicates if the instance is readonly (finished).
        /// </summary>
        public bool Readonly
        {
            get
            {
                var round = _draw.Keys.Last();

                return round == RoundEnum.F && _draw[round][0].Winner != null;
            }
        }

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
            _draw = new Dictionary<RoundEnum, IReadOnlyList<Match>>();

            var round = drawGen.DrawSize.GetRound();

            var drawRound = drawGen
                .GenerateDraw(drawTuple =>
                    // TODO : at this point, drawTuple.Item1 can be NULL and will throw an exception
                    new Match(availablePlayersRanked.ElementAtOrDefault(drawTuple.Item1),
                        availablePlayersRanked.ElementAtOrDefault(drawTuple.Item2),
                        Level.GetBestOf(),
                        FifthSetTieBreakRule,
                        Surface,
                        Level,
                        round,
                        Date))
                .ToList();

            _draw.Add(round, drawRound);
        }

        /// <summary>
        /// Proceeds to next round
        /// </summary>
        public void NextRound()
        {
            if (Readonly)
            {
                return;
            }

            var round = _draw.Keys.Last();

            foreach (Match match in _draw[round])
            {
                match.RunToEnd();
            }

            if (!Readonly)
            {
                var nextRound = (RoundEnum)(((int)round) - 1);
                var nextRoundMatches = new List<Match>();
                for (int i = 0; i < _draw[round].Count; i = i + 2)
                {
                    nextRoundMatches.Add(new Match(
                        _draw[round][i].Winner,
                        _draw[round][i + 1].Winner,
                        Level.GetBestOf(),
                        FifthSetTieBreakRule,
                        Surface,
                        Level,
                        nextRound,
                        Date));
                }

                _draw.Add(nextRound, nextRoundMatches);
            }
        }
    }
}
