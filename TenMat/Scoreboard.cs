using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data;

namespace TenMat
{
    /// <summary>
    /// Represents the scoreboard of a tennis match.
    /// </summary>
    public class Scoreboard
    {
        private static readonly int[] GamePoints = new int[] { 0, 15, 30, 40 };
        private readonly List<Set> _sets;
        private readonly int[] _currentGamePt;
        private readonly bool[] _currentGameAdv;

        /// <summary>
        /// Maximal number of sets.
        /// </summary>
        public int BestOf { get; }
        /// <summary>
        /// Index of the current server.
        /// </summary>
        public int CurrentServerIndex { get; private set; }
        /// <summary>
        /// Indicates the scoreboard is closed.
        /// </summary>
        public bool IsClosed { get; private set; }
        /// <summary>
        /// Tie-break rule for fifth set.
        /// </summary>
        public FifthSetTieBreakRuleEnum FifthSetTieBreakRule { get; private set; }

        /// <summary>
        /// Constructor (best of 3).
        /// </summary>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first server.</param>
        public Scoreboard(bool p2AtServe)
            : this(3, p2AtServe, FifthSetTieBreakRuleEnum.None) { }

        /// <summary>
        /// Constructor (best of 5).
        /// </summary>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first server.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        public Scoreboard(bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
            : this(5, p2AtServe, fifthSetTieBreakRule) { }

        /// <summary>
        /// Adds a point to the server.
        /// </summary>
        public void AddServerPoint()
        {
            AddPoint(CurrentServerIndex);
        }

        /// <summary>
        /// Adds a point to the receiver.
        /// </summary>
        public void AddReceiverPoint()
        {
            AddPoint(1 - CurrentServerIndex);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string setsScore = string.Join(" ", _sets.Select(set => string.Concat(set.Games1, "/", set.Games2)));
            string currentGame = string.Empty;
            string currentTb = string.Empty;
            if (_sets.Last().HasTieBreak)
            {
                currentTb = string.Concat(" | [", _sets.Last().TieBreakPoints1, "]-[", _sets.Last().TieBreakPoints2, "]");
            }
            else if (!IsClosed)
            {
                currentGame = string.Concat(" | ", _currentGamePt[0], _currentGameAdv[0] ? " (A)" : string.Empty, " - ", _currentGamePt[1], _currentGameAdv[1] ? " (A)" : string.Empty);
            }

            return string.Concat(setsScore, currentGame, currentTb);
        }

        private Scoreboard(byte bestOf, bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
        {
            BestOf = bestOf;
            FifthSetTieBreakRule = fifthSetTieBreakRule;
            _sets = new List<Set>
            {
                new Set()
            };
            CurrentServerIndex = p2AtServe ? 1 : 0;
            _currentGamePt = new int[2] { 0, 0 };
            _currentGameAdv = new bool[2] { false, false };
            IsClosed = false;
        }

        private void AddGame(int gameWinnerIndex)
        {
            _currentGamePt[0] = 0;
            _currentGamePt[1] = 0;
            _currentGameAdv[0] = false;
            _currentGameAdv[1] = false;

            var set = _sets.Last();
            set.AddGame(gameWinnerIndex);

            if (set.BothAt6 && (_sets.Count < 5 || FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At6_6))
            {
                _sets.Last().StartTieBreak();
            }
            else if (set.BothAt12 && _sets.Count == 5 && FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At12_12)
            {
                _sets.Last().StartTieBreak();
            }
            else if (_sets.Last().HasTieBreak || set.IsOverWithoutTieBreak)
            {
                if (_sets.Count(s => s.IsWonBy(gameWinnerIndex)) == (BestOf + 1) / 2)
                {
                    IsClosed = true;
                    return;
                }
                else
                {
                    _sets.Add(new Set());
                }
            }

            CurrentServerIndex = 1 - CurrentServerIndex;
        }

        private void AddPoint(int playerIndex)
        {
            if (IsClosed)
            {
                return;
            }

            if (_sets.Last().HasTieBreak)
            {
                var set = _sets.Last();

                set.AddTieBreakPoint(playerIndex);

                if (set.IsTieBreakOver)
                {
                    AddGame(playerIndex);
                }
                else if (set.IsTieBreakServerSwitch)
                {
                    CurrentServerIndex = 1 - CurrentServerIndex;
                }
            }
            else if (_currentGamePt[playerIndex] < GamePoints.Last())
            {
                _currentGamePt[playerIndex] = GamePoints[Array.IndexOf(GamePoints, _currentGamePt[playerIndex]) + 1];
            }
            else
            {
                if (_currentGameAdv[playerIndex])
                {
                    AddGame(playerIndex);
                }
                else if (_currentGameAdv[1 - playerIndex])
                {
                    _currentGameAdv[1 - playerIndex] = false;
                }
                else if (_currentGamePt[1 - playerIndex] < GamePoints.Last())
                {
                    AddGame(playerIndex);
                }
                else
                {
                    _currentGameAdv[playerIndex] = true;
                }
            }
        }
    }
}
