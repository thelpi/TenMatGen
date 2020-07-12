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
        private readonly int[] _playersPoints = new[] { GAME_POINTS[0], GAME_POINTS[0] };

        /// <summary>
        /// Gets points value for specified player.
        /// </summary>
        /// <param name="i">Player index.</param>
        /// <returns>Player points value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="i"/> should be 0 or 1.</exception>
        public int this[int i]
        {
            get
            {
                if (i < 0 || i > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(i), i, "Player index should be 0 or 1.");
                }

                return _playersPoints[i];
            }
        }

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
                    && _playersPoints[0] == GAME_POINTS[3]
                    && _playersPoints[1] == GAME_POINTS[3];
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
                if (_playersPoints[1] == GAME_POINTS[3])
                {
                    _readonly = true;
                    return true;
                }

                _playersPoints[1] = GAME_POINTS[Array.IndexOf(GAME_POINTS, _playersPoints[1]) + 1];
            }
            else
            {
                if (_playersPoints[0] == GAME_POINTS[3])
                {
                    _readonly = true;
                    return true;
                }

                _playersPoints[0] = GAME_POINTS[Array.IndexOf(GAME_POINTS, _playersPoints[0]) + 1];
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (_readonly)
            {
                return string.Empty;
            }

            return string.Concat(_playersPoints[0], AdvantageToString(0), " - ", _playersPoints[1], AdvantageToString(1));
        }

        private string AdvantageToString(int playerIndex)
        {
            return AdvantagePlayerIndex == playerIndex ? " (A)" : string.Empty;
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
