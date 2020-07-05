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
        private readonly List<int[]> _sets;
        private readonly List<int?> _tieBreaks;
        private readonly int[] _currentGamePt;
        private readonly bool[] _currentGameAdv;
        private readonly int[] _currentTieBreakValues;

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
        /// List of sets details.
        /// </summary>
        public IReadOnlyCollection<int[]> Sets { get { return _sets; } }

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
            string setsScore = string.Join(" ", _sets.Select(set => string.Concat(set[0], "/", set[1])));
            string currentGame = string.Empty;
            string currentTb = string.Empty;
            if (_tieBreaks.Last().HasValue)
            {
                currentTb = string.Concat(" | [", _currentTieBreakValues[0], "]-[", _currentTieBreakValues[1], "]");
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
            _sets = new List<int[]>
            {
                new [] { 0, 0 }
            };
            _tieBreaks = new List<int?> { null };
            _currentTieBreakValues = new int[2] { 0, 0 };
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

            var currentSetDatas = _sets.Last();
            currentSetDatas[gameWinnerIndex]++;

            if (currentSetDatas.All(v => v == 6) && (_sets.Count < 5 || FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At6_6))
            {
                _tieBreaks[_tieBreaks.Count - 1] = 0;
            }
            else if (currentSetDatas.All(v => v == 12) && _sets.Count == 5 && FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At12_12)
            {
                _tieBreaks[_tieBreaks.Count - 1] = 0;
            }
            else if (_tieBreaks.Last().HasValue || (currentSetDatas.Any(v => v >= 6) && Math.Abs(currentSetDatas[0] - currentSetDatas[1]) > 1))
            {
                if (_sets.Count(set => set[gameWinnerIndex] > set[1 - gameWinnerIndex]) == (BestOf + 1) / 2)
                {
                    IsClosed = true;
                    return;
                }
                else
                {
                    _sets.Add(new[] { 0, 0 });
                    _tieBreaks.Add(null);
                    _currentTieBreakValues[0] = 0;
                    _currentTieBreakValues[1] = 0;
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

            if (_tieBreaks.Last().HasValue)
            {
                _currentTieBreakValues[playerIndex]++;

                _tieBreaks[_tieBreaks.Count - 1] = _currentTieBreakValues[1 - playerIndex];

                if ((_currentTieBreakValues[playerIndex] == 7
                    && _currentTieBreakValues[1 - playerIndex] <= 5) || (_currentTieBreakValues[playerIndex] > 7
                    && _currentTieBreakValues[1 - playerIndex] <= _currentTieBreakValues[playerIndex] - 2))
                {
                    AddGame(playerIndex);
                }
                else if (_currentTieBreakValues.Sum() % 2 == 1)
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
