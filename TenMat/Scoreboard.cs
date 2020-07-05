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
        private readonly List<Set> _sets;

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
                currentGame = string.Concat(" | ", _sets.Last().CurrentGame.Points1, _sets.Last().CurrentGame.AdvantagePlayerIndex == 0 ? " (A)" : string.Empty, " - ", _sets.Last().CurrentGame.Points2, _sets.Last().CurrentGame.AdvantagePlayerIndex == 1 ? " (A)" : string.Empty);
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
            IsClosed = false;
        }

        private void AddPoint(int playerIndex)
        {
            if (IsClosed)
            {
                return;
            }

            var set = _sets.Last();

            bool newSet = set.AddPoint(playerIndex, _sets.Count == 5, FifthSetTieBreakRule, out bool switchServer);

            if (switchServer)
            {
                CurrentServerIndex = 1 - CurrentServerIndex;
            }

            if (newSet)
            {
                if (_sets.Count(s => s.IsWonBy(playerIndex)) == (BestOf + 1) / 2)
                {
                    IsClosed = true;
                    return;
                }
                else
                {
                    _sets.Add(new Set());
                }
            }
        }
    }
}
