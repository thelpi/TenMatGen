using System;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a set.
    /// </summary>
    public class Set
    {
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
        /// <c>True</c> if both players have six games.
        /// </summary>
        public bool BothAt6
        {
            get
            {
                return Games1 == 6 && Games2 == 6;
            }
        }

        /// <summary>
        /// <c>True</c> if both players have twelve games.
        /// </summary>
        public bool BothAt12
        {
            get
            {
                return Games1 == 12 && Games2 == 12;
            }
        }

        /// <summary>
        /// <c>True</c> if the set is over without tie-break.
        /// </summary>
        public bool IsOverWithoutTieBreak
        {
            get
            {
                return (Games1 >= 6 || Games2 >= 6) && Math.Abs(Games1 - Games2) > 1;
            }
        }

        /// <summary>
        /// Indicates if the server has to switch during the current tie-break.
        /// </summary>
        public bool IsTieBreakServerSwitch
        {
            get
            {
                return (TieBreakPoints1 + TieBreakPoints2) % 2 == 1;
            }
        }

        /// <summary>
        /// Is tie-break over.
        /// </summary>
        public bool IsTieBreakOver
        {
            get
            {
                return (TieBreakPoints1 == 7 && TieBreakPoints2 <= 5)
                    || (TieBreakPoints1 > 7 && TieBreakPoints2 <= TieBreakPoints1 - 2)
                    || (TieBreakPoints2 == 7 && TieBreakPoints1 <= 5)
                    || (TieBreakPoints2 > 7 && TieBreakPoints1 <= TieBreakPoints2 - 2);
            }
        }

        /// <summary>
        /// Adds a game.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        public void AddGame(int playerIndex)
        {
            CheckPlayerIndex(playerIndex);

            if (playerIndex == 1)
            {
                Games2++;
            }
            else
            {
                Games1++;
            }
        }

        /// <summary>
        /// Adds a tie-break point.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        public void AddTieBreakPoint(int playerIndex)
        {
            CheckPlayerIndex(playerIndex);
            
            if (playerIndex == 1)
            {
                TieBreakPoints2++;
            }
            else
            {
                TieBreakPoints1++;
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
        /// Starts a tie-break.
        /// </summary>
        public void StartTieBreak()
        {
            HasTieBreak = true;
        }

        private static void CheckPlayerIndex(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex), playerIndex, "The value should be one or zero.");
            }
        }
    }
}
