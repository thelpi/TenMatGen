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

        private readonly int[] _playersGames = new int[] { 0, 0 };
        private readonly int[] _playersTieBreakPoints = new int[] { 0, 0 };

        /// <summary>
        /// Indicates if readonly (finished).
        /// </summary>
        public bool Readonly { get; private set; }
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
        /// Games count.
        /// </summary>
        public int GamesCount
        {
            get
            {
                return _playersGames[0] + _playersGames[1];
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
                _playersGames[0] > _playersGames[1] :
                _playersGames[1] > _playersGames[0];
        }

        /// <summary>
        /// Adds a point to the current game.
        /// </summary>
        /// <param name="playerIndex">Player index.</param>
        /// <param name="isFifthSet"><c>True</c> if it's the fifth set.</param>
        /// <param name="fifthSetTieBreakRule">The tie-break rule for the fifth set.</param>
        /// <param name="switchServer">Out; <c>True</c> if server has to switch.</param>
        /// <param name="newGame">Out; <c>True</c> if a new game begins.</param>
        /// <returns><c>True</c> if the set ends.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerIndex"/> should be one or zero.</exception>
        /// <exception cref="InvalidOperationException">The instance is read-only.</exception>
        public bool AddPoint(int playerIndex, bool isFifthSet, FifthSetTieBreakRuleEnum fifthSetTieBreakRule, out bool switchServer, out bool newGame)
        {
            CheckPlayerIndex(playerIndex);

            if (Readonly)
            {
                throw new InvalidOperationException("The instance is read-only.");
            }

            switchServer = false;
            newGame = false;

            if (HasTieBreak)
            {
                if (playerIndex == 1)
                {
                    _playersTieBreakPoints[1]++;
                }
                else
                {
                    _playersTieBreakPoints[0]++;
                }
                if ((_playersTieBreakPoints[0] == 7 && _playersTieBreakPoints[1] <= 5)
                    || (_playersTieBreakPoints[0] > 7 && _playersTieBreakPoints[1] <= _playersTieBreakPoints[0] - 2)
                    || (_playersTieBreakPoints[1] == 7 && _playersTieBreakPoints[0] <= 5)
                    || (_playersTieBreakPoints[1] > 7 && _playersTieBreakPoints[0] <= _playersTieBreakPoints[1] - 2))
                {
                    switchServer = true;
                    newGame = true;
                    return AddGame(playerIndex, isFifthSet, fifthSetTieBreakRule);
                }
                else if ((_playersTieBreakPoints[0] + _playersTieBreakPoints[1]) % 2 == 1)
                {
                    switchServer = true;
                }
            }
            else if (CurrentGame.AddPoint(playerIndex))
            {
                switchServer = true;
                newGame = true;
                return AddGame(playerIndex, isFifthSet, fifthSetTieBreakRule);
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Readonly)
            {
                return string.Concat(_playersGames[0], TieBreakToString(_playersTieBreakPoints[0]),
                    "/", _playersGames[1], TieBreakToString(_playersTieBreakPoints[1]));
            }

            var baseString = string.Concat(_playersGames[0], "/", _playersGames[1]);
            if (HasTieBreak)
            {
                baseString += string.Concat(" | [", _playersTieBreakPoints[0], "]-[", _playersTieBreakPoints[1], "]");
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
                && Math.Min(_playersTieBreakPoints[0], _playersTieBreakPoints[1]) == tieBreakPoints ?
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
                _playersGames[1]++;
            }
            else
            {
                _playersGames[0]++;
            }

            _games.Add(new Game());

            if ((_playersGames[0] == 6 && _playersGames[1] == 6) && (!isFifthSet || fifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At6_6))
            {
                HasTieBreak = true;
            }
            else if ((_playersGames[0] == 12 && _playersGames[1] == 12) && isFifthSet && fifthSetTieBreakRule == FifthSetTieBreakRuleEnum.At12_12)
            {
                HasTieBreak = true;
            }
            else if (HasTieBreak || ((_playersGames[0] >= 6 || _playersGames[1] >= 6) && Math.Abs(_playersGames[0] - _playersGames[1]) > 1))
            {
                Readonly = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a <see cref="Set"/> instance from a match archive.
        /// </summary>
        /// <param name="winnerGamesCount">Winner games count.</param>
        /// <param name="loserGamesCount">Loser games count.</param>
        /// <param name="loserTieBreakPoints">Loser tie-break points.</param>
        /// <returns><see cref="Set"/> instance.</returns>
        public static Set CreateFromArchive(uint winnerGamesCount, uint loserGamesCount, uint? loserTieBreakPoints)
        {
            var set = new Set
            {
                Readonly = true,
                HasTieBreak = loserTieBreakPoints.HasValue
            };
            set._playersGames[0] = (int)winnerGamesCount;
            set._playersGames[1] = (int)loserGamesCount;
            set._playersTieBreakPoints[0] = loserTieBreakPoints.HasValue ? (int)(loserTieBreakPoints.Value > 5 ? loserTieBreakPoints.Value + 2 : 7) : 0;
            set._playersTieBreakPoints[1] = (int)loserTieBreakPoints.GetValueOrDefault(0);
            return set;
        }
    }
}
