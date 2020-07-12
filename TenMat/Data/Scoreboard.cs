using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents the scoreboard of a tennis match.
    /// </summary>
    public class Scoreboard
    {
        private readonly List<Set> _sets = new List<Set> { new Set() };

        /// <summary>
        /// <see cref="BestOfEnum"/> value.
        /// </summary>
        public BestOfEnum BestOf { get; }
        /// <summary>
        /// Tie-break rule for fifth set.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; }
        /// <summary>
        /// Index of the current server.
        /// </summary>
        public int CurrentServerIndex { get; private set; }
        /// <summary>
        /// Indicates if the scoring is made point by point or game by game.
        /// </summary>
        public bool PointByPoint { get; }

        /// <summary>
        /// Indicates if readonly (finished).
        /// </summary>
        public bool Readonly { get { return _sets.Last().Readonly; } }

        /// <summary>
        /// Gets the index (0 or 1) of the winner (if finished) or the current leader otherwise; <c>-1</c> if there's no leader at this point.
        /// </summary>
        public int IndexLead
        {
            get
            {
                int setsCount0 = _sets.Count(s => s.IsWonBy(0));
                int setsCount1 = _sets.Count(s => s.IsWonBy(1));

                if (setsCount0 > setsCount1)
                {
                    return 0;
                }
                else if (setsCount1 > setsCount0)
                {
                    return 1;
                }

                if (_sets.Last().Games1 > _sets.Last().Games2)
                {
                    return 0;
                }
                else if (_sets.Last().Games1 < _sets.Last().Games2)
                {
                    return 1;
                }

                if (_sets.Last().CurrentGame != null)
                {
                    if (_sets.Last().CurrentGame[0] > _sets.Last().CurrentGame[1])
                    {
                        return 0;
                    }
                    else if (_sets.Last().CurrentGame[1] > _sets.Last().CurrentGame[0])
                    {
                        return 1;
                    }
                    else if (_sets.Last().CurrentGame.AdvantagePlayerIndex == 0)
                    {
                        return 0;
                    }
                    else if (_sets.Last().CurrentGame.AdvantagePlayerIndex == 1)
                    {
                        return 1;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Indicates if the current set is in tie-break.
        /// </summary>
        public bool IsCurrentlyTieBreak
        {
            get
            {
                return _sets.Last().HasTieBreak;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bestOf">The <see cref="BestOf"/> value.</param>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first to serve.</param>
        /// <param name="fifthSetTieBreakRule">The <see cref="FifthSetTieBreakRule"/> value.</param>
        /// <param name="pointByPoint">The <see cref="PointByPoint"/> value.</param>
        public Scoreboard(BestOfEnum bestOf, bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule, bool pointByPoint)
        {
            BestOf = bestOf;
            FifthSetTieBreakRule = fifthSetTieBreakRule;
            CurrentServerIndex = p2AtServe ? 1 : 0;
            PointByPoint = pointByPoint;
        }

        /// <summary>
        /// Adds a point to the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The scoreboard is not set point by point.</exception>
        /// <exception cref="InvalidOperationException">The instance is readonly.</exception>
        public void AddServerPoint()
        {
            if (!PointByPoint)
            {
                throw new InvalidOperationException("The scoreboard is not set point by point.");
            }

            AddPoint(CurrentServerIndex);
        }

        /// <summary>
        /// Adds a point to the receiver.
        /// </summary>
        /// <exception cref="InvalidOperationException">The scoreboard is not set point by point.</exception>
        /// <exception cref="InvalidOperationException">The instance is readonly.</exception>
        public void AddReceiverPoint()
        {
            if (!PointByPoint)
            {
                throw new InvalidOperationException("The scoreboard is not set point by point.");
            }

            AddPoint(1 - CurrentServerIndex);
        }

        /// <summary>
        /// Gives the gain of the current game to the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The scoreboard is set point by point.</exception>
        /// <exception cref="InvalidOperationException">The instance is readonly.</exception>
        /// <exception cref="InvalidOperationException">Can't call this method while in tie-break.</exception>
        public void AddServerGame()
        {
            if (PointByPoint)
            {
                throw new InvalidOperationException("The scoreboard is set point by point.");
            }

            if (IsCurrentlyTieBreak)
            {
                throw new InvalidOperationException("Can't call this method while in tie-break.");
            }

            while (!AddPoint(CurrentServerIndex)) { }
        }

        /// <summary>
        /// Gives the gain of the current game to the receiver.
        /// </summary>
        /// <exception cref="InvalidOperationException">The scoreboard is set point by point.</exception>
        /// <exception cref="InvalidOperationException">The instance is readonly.</exception>
        /// <exception cref="InvalidOperationException">Can't call this method while in tie-break.</exception>
        public void AddReceiverGame()
        {
            if (PointByPoint)
            {
                throw new InvalidOperationException("The scoreboard is set point by point.");
            }

            if (IsCurrentlyTieBreak)
            {
                throw new InvalidOperationException("Can't call this method while in tie-break.");
            }

            while (!AddPoint(1 - CurrentServerIndex)) { }
        }

        /// <summary>
        /// Gives the tie-break to the specified player index.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <exception cref="InvalidOperationException">The scoreboard is set point by point.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be 0 or 1.</exception>
        /// <exception cref="InvalidOperationException">The instance is readonly.</exception>
        public void AddTieBreak(int playerIndex)
        {
            if (PointByPoint)
            {
                throw new InvalidOperationException("The scoreboard is set point by point.");
            }

            if (playerIndex < 0 || playerIndex > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex), playerIndex, "Player index should be 0 or 1.");
            }

            while (!AddPoint(playerIndex)) { }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" - ", _sets.Select(set => set.ToString()));
        }

        private bool AddPoint(int playerIndex)
        {
            if (Readonly)
            {
                throw new InvalidOperationException("The instance is readonly.");
            }

            var set = _sets.Last();

            bool newSet = set.AddPoint(playerIndex, _sets.Count == 5, FifthSetTieBreakRule, out bool switchServer, out bool newGame);

            if (switchServer)
            {
                CurrentServerIndex = 1 - CurrentServerIndex;
            }
            if (newSet && _sets.Count(s => s.IsWonBy(playerIndex)) != ((int)BestOf + 1) / 2)
            {
                _sets.Add(new Set());
            }

            return newGame;
        }
    }
}
