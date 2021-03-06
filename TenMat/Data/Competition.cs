﻿using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a competition.
    /// </summary>
    /// <seealso cref="CompetitionBase"/>
    public class Competition : CompetitionBase
    {
        private readonly Dictionary<RoundEnum, IReadOnlyList<Match>> _draw;

        /// <summary>
        /// Rule for fifth set tie-break.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }
        /// <summary>
        /// The <see cref="Scoreboard.PointByPoint"/> value for every matches.
        /// </summary>
        public bool PointByPoint { get; }

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

                if (_draw[round].Count == 0)
                {
                    throw new InvalidOperationException("Should not happen.");
                }

                return round == RoundEnum.F && _draw[round][0].Winner != null;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drawGen">Instance of <see cref="DrawGenerator"/>.</param>
        /// <param name="date">The <see cref="CompetitionBase.Date"/> value.</param>
        /// <param name="level">The <see cref="CompetitionBase.Level"/> value.</param>
        /// <param name="fifthSetTieBreakRule">The <see cref="FifthSetTieBreakRule"/> value.</param>
        /// <param name="surface">The <see cref="CompetitionBase.Surface"/> value.</param>
        /// <param name="isIndoor">The <see cref="CompetitionBase.IsIndoor"/> value.</param>
        /// <param name="availablePlayersRanked">List of available players, sorted by ranking.</param>
        /// <param name="bestOf">The <see cref="CompetitionBase.BestOf"/> value.</param>
        /// <param name="finalBestOf">The <see cref="CompetitionBase.FinalBestOf"/> value.</param>
        /// <param name="pointByPoint">The <see cref="PointByPoint"/> value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="drawGen"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="availablePlayersRanked"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="availablePlayersRanked"/> should not contain duplicates.</exception>
        /// <exception cref="ArgumentException">The list of players should contains at least two elements.</exception>
        public Competition(DrawGenerator drawGen, DateTime date, LevelEnum level,
            FifthSetTieBreakRuleEnum fifthSetTieBreakRule, SurfaceEnum surface,
            bool isIndoor, IEnumerable<Player> availablePlayersRanked,
            BestOfEnum bestOf, BestOfEnum finalBestOf, bool pointByPoint)
            : base(surface, isIndoor, level, date)
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

            if (finalBestOf == BestOfEnum.Five)
            {
                SetBestOfFive(true);
            }
            if (bestOf == BestOfEnum.Five)
            {
                SetBestOfFive(false);
            }
            PointByPoint = pointByPoint;
            FifthSetTieBreakRule = fifthSetTieBreakRule;
            _draw = new Dictionary<RoundEnum, IReadOnlyList<Match>>();

            var round = Tools.GetFirstRound(drawGen.DrawSize);

            List<Match> drawRound = drawGen
                .GenerateDraw(drawTuple =>
                    // TODO : at this point, drawTuple.Item1 can be NULL and will throw an exception
                    NewMatch(availablePlayersRanked.ElementAtOrDefault(drawTuple.Item1),
                        availablePlayersRanked.ElementAtOrDefault(drawTuple.Item2),
                        round))
                .ToList();

            if (drawRound.Count == 0)
            {
                throw new InvalidOperationException("Should not happen.");
            }

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
                    nextRoundMatches.Add(NewMatch(_draw[round][i].Winner,
                        _draw[round][i + 1].Winner,
                        nextRound));
                }

                if (nextRoundMatches.Count == 0)
                {
                    throw new InvalidOperationException("Should not happen.");
                }

                _draw.Add(nextRound, nextRoundMatches);
            }
        }

        private Match NewMatch(Player p1, Player p2, RoundEnum round)
        {
            return Match.CreateNew(p1, p2, this, round);
        }
    }
}
