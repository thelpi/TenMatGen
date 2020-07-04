using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat
{
    /// <summary>
    /// Represents the scoreboard of a tennis match.
    /// </summary>
    /// <remarks>No tie-break at this point.</remarks>
    public class MatchScoreboard
    {
        /// <summary>
        /// Sorted list of points thresholds available during a single game.
        /// </summary>
        public static readonly int[] GamePoints = new int[] { 0, 15, 30, 40 };

        private readonly List<int[]> _sets;
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
        /// List of sets details.
        /// </summary>
        public IReadOnlyCollection<int[]> Sets { get { return _sets; } }

        /// <summary>
        /// Constructor (best of 3).
        /// </summary>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first server.</param>
        public MatchScoreboard(bool p2AtServe)
            : this(3, p2AtServe, FifthSetTieBreakRuleEnum.None) { }

        /// <summary>
        /// Constructor (best of 5).
        /// </summary>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first server.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        public MatchScoreboard(bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
            : this(5, p2AtServe, fifthSetTieBreakRule) { }

        private MatchScoreboard(byte bestOf, bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
        {
            BestOf = bestOf;
            FifthSetTieBreakRule = fifthSetTieBreakRule;
            _sets = new List<int[]>
            {
                new [] { 0, 0 }
            };
            CurrentServerIndex = p2AtServe ? 1 : 0;
            _currentGamePt = new int[2] { 0, 0 };
            _currentGameAdv = new bool[2] { false, false };
            IsClosed = false;
        }

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

        private void AddGame(int gameWinnerIndex)
        {
            _currentGamePt[0] = 0;
            _currentGamePt[1] = 0;
            _currentGameAdv[0] = false;
            _currentGameAdv[1] = false;

            var currentSetDatas = _sets.Last();
            currentSetDatas[gameWinnerIndex]++;

            /*if (currentSetDatas.All(v => v == 6) && (_sets.Count < 5 || FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At6_6))
            {

            }
            else if (currentSetDatas.All(v => v == 12) && _sets.Count == 5 && FifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At12_12)
            {

            }
            else */
            if (currentSetDatas.Any(v => v >= 6) && Math.Abs(currentSetDatas[0] - currentSetDatas[1]) > 1)
            {
                if (_sets.Count(set => set[gameWinnerIndex] > set[1 - gameWinnerIndex]) == (BestOf + 1) / 2)
                {
                    IsClosed = true;
                    return;
                }
                else
                {
                    _sets.Add(new[] { 0, 0 });
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

            if (_currentGamePt[playerIndex] < GamePoints.Last())
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

        public override string ToString()
        {
            return string.Concat(string.Join(" ", _sets.Select(set => string.Concat(set[0], "/", set[1]))), " | ", _currentGamePt[0], _currentGameAdv[0] ? " (A)" : string.Empty, " - ", _currentGamePt[1], _currentGameAdv[1] ? " (A)" : string.Empty);
        }

        /// <summary>
        /// Fifth set tie-break rules enumeration.
        /// </summary>
        public enum FifthSetTieBreakRuleEnum
        {
            /// <summary>
            /// No tie-break / not applicable.
            /// </summary>
            None,
            /// <summary>
            /// Tie-break at 6 - 6.
            /// </summary>
            At6_6,
            /// <summary>
            /// Tie-break at 12 - 12.
            /// </summary>
            At12_12
        }
    }
}
