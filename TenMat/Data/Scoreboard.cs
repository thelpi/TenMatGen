using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Indicates if readonly (finished).
        /// </summary>
        public bool Readonly { get { return _sets.Last().Readonly; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bestOf"><see cref="BestOf"/> value.</param>
        /// <param name="p2AtServe"><c>True</c> if the second player is the first to serve.</param>
        /// <param name="fifthSetTieBreakRule"><see cref="FifthSetTieBreakRule"/> value.</param>
        public Scoreboard(BestOfEnum bestOf, bool p2AtServe, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
        {
            BestOf = bestOf;
            FifthSetTieBreakRule = fifthSetTieBreakRule;
            CurrentServerIndex = p2AtServe ? 1 : 0;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" - ", _sets.Select(set => set.ToString()));
        }

        private void AddPoint(int playerIndex)
        {
            if (Readonly)
            {
                throw new InvalidOperationException("The instance is readonly.");
            }

            var set = _sets.Last();

            bool newSet = set.AddPoint(playerIndex, _sets.Count == 5, FifthSetTieBreakRule, out bool switchServer);

            if (switchServer)
            {
                CurrentServerIndex = 1 - CurrentServerIndex;
            }
            if (newSet && _sets.Count(s => s.IsWonBy(playerIndex)) != ((int)BestOf + 1) / 2)
            {
                _sets.Add(new Set());
            }
        }
    }
}
