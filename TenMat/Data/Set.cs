using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a set.
    /// </summary>
    public class Set
    {
        private readonly List<Game> _games = new List<Game>
        {
            new Game()
        };

        /// <summary>
        /// Indicates if readonly (finished).
        /// </summary>
        public bool Readonly { get; private set; }
        /// <summary>
        /// Games for player one.
        /// </summary>
        public int Games1 { get; private set; }
        /// <summary>
        /// Games for player two.
        /// </summary>
        public int Games2 { get; private set; }
        /// <summary>
        /// Tie-break points for player one.
        /// </summary>
        public int TieBreakPoints1 { get; private set; }
        /// <summary>
        /// Tie-break points for player two.
        /// </summary>
        public int TieBreakPoints2 { get; private set; }
        /// <summary>
        /// Indicates if the set ahs a tie-break.
        /// </summary>
        public bool HasTieBreak { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Game"/>.
        /// </summary>
        public Game CurrentGame
        {
            get
            {
                return _games.Last();
            }
        }

        /// <summary>
        /// Indicates if the specified player has won the set.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <returns><c>True</c> if <paramref name="playerIndex"/> has won of the set.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        public bool IsWonBy(int playerIndex)
        {
            CheckPlayerIndex(playerIndex);

            return playerIndex == 0 ?
                Games1 > Games2 :
                Games2 > Games1;
        }

        /// <summary>
        /// Adds a point to the current game.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <param name="isFifthSet"><c>True</c> if it's the fifth set.</param>
        /// <param name="fifthSetTieBreakRule">The tie-break rule for the fifth set.</param>
        /// <param name="switchServer">Out; <c>True</c> if server has to switch.</param>
        /// <returns><c>True</c> if the set ends.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        /// <exception cref="InvalidOperationException">The instance is read-only.</exception>
        public bool AddPoint(int playerIndex, bool isFifthSet, FifthSetTieBreakRuleEnum fifthSetTieBreakRule, out bool switchServer)
        {
            CheckPlayerIndex(playerIndex);

            if (Readonly)
            {
                throw new InvalidOperationException("The instance is read-only.");
            }

            switchServer = false;

            if (HasTieBreak)
            {
                if (playerIndex == 1)
                {
                    TieBreakPoints2++;
                }
                else
                {
                    TieBreakPoints1++;
                }
                if ((TieBreakPoints1 == 7 && TieBreakPoints2 <= 5)
                    || (TieBreakPoints1 > 7 && TieBreakPoints2 <= TieBreakPoints1 - 2)
                    || (TieBreakPoints2 == 7 && TieBreakPoints1 <= 5)
                    || (TieBreakPoints2 > 7 && TieBreakPoints1 <= TieBreakPoints2 - 2))
                {
                    switchServer = true;
                    return AddGame(playerIndex, isFifthSet, fifthSetTieBreakRule);
                }
                else if ((TieBreakPoints1 + TieBreakPoints2) % 2 == 1)
                {
                    switchServer = true;
                }
            }
            else if (CurrentGame.AddPoint(playerIndex))
            {
                switchServer = true;
                return AddGame(playerIndex, isFifthSet, fifthSetTieBreakRule);
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Readonly)
            {
                return string.Concat(Games1, TieBreakToString(TieBreakPoints1),
                    "/", Games2, TieBreakToString(TieBreakPoints2));
            }

            var baseString = string.Concat(Games1, "/", Games2);
            if (HasTieBreak)
            {
                baseString += string.Concat(" | [", TieBreakPoints1, "]-[", TieBreakPoints2, "]");
            }
            else
            {
                baseString += string.Concat(" | ", CurrentGame.ToString());
            }

            return baseString;
        }

        private string TieBreakToString(int tieBreakPoints)
        {
            return HasTieBreak
                && Math.Min(TieBreakPoints1, TieBreakPoints2) == tieBreakPoints ?
                    string.Concat("[", tieBreakPoints, "]") : string.Empty;
        }

        private static void CheckPlayerIndex(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex), playerIndex, "The value should be one or zero.");
            }
        }

        private bool AddGame(int playerIndex, bool isFifthSet, FifthSetTieBreakRuleEnum fifthSetTieBreakRule)
        {
            if (playerIndex == 1)
            {
                Games2++;
            }
            else
            {
                Games1++;
            }

            _games.Add(new Game());

            if ((Games1 == 6 && Games2 == 6) && (!isFifthSet || fifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At6_6))
            {
                HasTieBreak = true;
            }
            else if ((Games1 == 12 && Games2 == 12) && isFifthSet && fifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At12_12)
            {
                HasTieBreak = true;
            }
            else if (HasTieBreak || ((Games1 >= 6 || Games2 >= 6) && Math.Abs(Games1 - Games2) > 1))
            {
                Readonly = true;
                return true;
            }

            return false;
        }
    }
}
