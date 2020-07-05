using System;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a game.
    /// </summary>
    public class Game
    {
        private static readonly int[] GAME_POINTS = new int[] { 0, 15, 30, 40 };

        private bool _readonly;

        /// <summary>
        /// Points for player one.
        /// </summary>
        public int Points1 { get; private set; }
        /// <summary>
        /// Points for player two.
        /// </summary>
        public int Points2 { get; private set; }
        /// <summary>
        /// Indicates if a player has advantage at 40/40.
        /// </summary>
        public int? AdvantagePlayerIndex { get; private set; }

        /// <summary>
        /// Indicates if deuce.
        /// </summary>
        public bool IsDeuce
        {
            get
            {
                return AdvantagePlayerIndex == null
                    && Points1 == GAME_POINTS[3]
                    && Points2 == GAME_POINTS[3];
            }
        }

        /// <summary>
        /// Adds a point.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <returns><c>True</c> if the game is over.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        /// <exception cref="InvalidOperationException">The instance is read-only.</exception>
        public bool AddPoint(int playerIndex)
        {
            CheckPlayerIndex(playerIndex);

            if (_readonly)
            {
                throw new InvalidOperationException("The instance is read-only.");
            }

            if (AdvantagePlayerIndex == playerIndex)
            {
                _readonly = true;
                return true;
            }

            if (AdvantagePlayerIndex == 1 - playerIndex)
            {
                AdvantagePlayerIndex = null;
            }
            else if (IsDeuce)
            {
                AdvantagePlayerIndex = playerIndex;
            }
            else if (playerIndex == 1)
            {
                if (Points2 == GAME_POINTS[3])
                {
                    _readonly = true;
                    return true;
                }

                Points2 = GAME_POINTS[Array.IndexOf(GAME_POINTS, Points2) + 1];
            }
            else
            {
                if (Points1 == GAME_POINTS[3])
                {
                    _readonly = true;
                    return true;
                }

                Points1 = GAME_POINTS[Array.IndexOf(GAME_POINTS, Points1) + 1];
            }

            return false;
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
