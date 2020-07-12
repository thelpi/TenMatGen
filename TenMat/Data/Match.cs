using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a match.
    /// </summary>
    /// <seealso cref="MatchBase"/>
    public class Match : MatchBase
    {
        private const string EXEMPT_PLAYER_NAME = "Exempt";
        private const double DEF_SERVE_RATE = 0.7;

        private readonly Scoreboard _scoreboard;
        private readonly Player _playerOne;
        private readonly Player _playerTwo;
        private readonly double _p1ServeRate;
        private readonly double _p2ServeRate;
        private readonly double _p1TieBreakRate;
        private readonly bool _p2IsFirstToServe;

        /// <summary>
        /// Gets the winner of the match (if finished).
        /// </summary>
        public Player Winner
        {
            get
            {
                if (_playerTwo == null)
                {
                    return _playerOne;
                }

                if (!_scoreboard.Readonly || _scoreboard.IndexLead == -1)
                {
                    return null;
                }

                return _scoreboard.IndexLead == 0 ? _playerOne : _playerTwo;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="Match"/>.
        /// </summary>
        /// <param name="p1">First <see cref="Player"/>.</param>
        /// <param name="p2">Second <see cref="Player"/>.</param>
        /// <param name="competition">Instance of <see cref="Competition"/>.</param>
        /// <param name="round">Current <see cref="RoundEnum"/>.</param>
        /// <returns>Instance of <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="competition"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="p1"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Players should not be the same.</exception>
        public static Match CreateNew(Player p1, Player p2, Competition competition, RoundEnum round)
        {
            if (competition == null)
            {
                throw new ArgumentNullException(nameof(competition));
            }

            if (p1 == null)
            {
                throw new ArgumentNullException(nameof(p1));
            }

            if (p2?.Id == p1.Id)
            {
                throw new ArgumentException("Players should not be the same.", nameof(p2));
            }

            return new Match(p1, p2, competition.Surface, competition.Level, round,
                round == RoundEnum.F ? competition.FinalBestOf : competition.BestOf,
                competition.Date, competition.FifthSetTieBreakRule, competition.PointByPoint);
        }
        
        private Match(Player p1, Player p2, SurfaceEnum surface, LevelEnum level,
            RoundEnum round, BestOfEnum bestOf, DateTime tournamentBeginningDate,
            FifthSetTieBreakRuleEnum fifthSetTieBreakRule, bool pointByPoint)
            : base(surface, level, round, bestOf, tournamentBeginningDate)
        {
            _p2IsFirstToServe = Tools.FlipCoin();
            _playerOne = p1;
            _playerTwo = p2;
            _p1TieBreakRate = 1;
            if (p2 != null)
            {
                _scoreboard = new Scoreboard(bestOf, _p2IsFirstToServe, fifthSetTieBreakRule, pointByPoint);
                ComputeServeRate(out _p1ServeRate, out _p2ServeRate);
                _p1TieBreakRate = 0.5; // TODO
            }
        }

        /// <summary>
        /// Runs match points to the end.
        /// </summary>
        public void RunToEnd()
        {
            if (_playerTwo == null)
            {
                return;
            }

            while (!_scoreboard.Readonly)
            {
                if (_scoreboard.PointByPoint)
                {
                    if (Tools.Rdm.NextDouble() >= GetCurrentPlayerServeRate())
                    {
                        _scoreboard.AddReceiverPoint();
                    }
                    else
                    {
                        _scoreboard.AddServerPoint();
                    }
                }
                else
                {
                    if (_scoreboard.IsCurrentlyTieBreak)
                    {
                        _scoreboard.AddTieBreak(
                            Tools.Rdm.NextDouble() >= _p1TieBreakRate ?
                                1 : 0);
                    }
                    else
                    {
                        if (Tools.Rdm.NextDouble() >= GetCurrentPlayerServeRate())
                        {
                            _scoreboard.AddReceiverGame();
                        }
                        else
                        {
                            _scoreboard.AddServerGame();
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string p2Name = _playerTwo?.Name ?? EXEMPT_PLAYER_NAME;

            var sb = new StringBuilder();
            sb.AppendLine(string.Concat(_playerOne.Name, " - ", _playerTwo.Name));
            sb.AppendLine(string.Concat(Surface, " - ", Level, " - ", Round));
            if (_playerTwo != null)
            {
                sb.AppendLine(_scoreboard.ToString());
            }
            return sb.ToString();
        }

        private void ComputeServeRate(out double player1Rate, out double player2Rate)
        {
            player1Rate = DEF_SERVE_RATE;
            player2Rate = DEF_SERVE_RATE;

            double p1Rate = ComputeRatio(_playerOne, _playerTwo);
            double p2Rate = ComputeRatio(_playerTwo, _playerOne);
            double delta = p1Rate / (p1Rate + p2Rate);

            if (delta > 0.5)
            {
                player1Rate += (delta * 0.2);
                player2Rate -= ((1 - delta) * 0.2);
            }
            else if (delta < 0.5)
            {
                player1Rate -= (delta * 0.2);
                player2Rate += ((1 - delta) * 0.2);
            }
        }

        private double ComputeRatio(Player p, Player opp)
        {
            var rates = new double?[]
            {
                p.WinRateByLevel[Level],
                p.WinRateByRound[Round],
                p.WinRateByBestOf[_scoreboard.BestOf],
                p.WinRateBySurface[Surface],
                p.WinRateByOpponent.ContainsKey(opp.Id) ? p.WinRateByOpponent[opp.Id] : null,
                p.WinRateByYear.ContainsKey(TournamentBeginningDate.Year) ? p.WinRateByYear[TournamentBeginningDate.Year] : null
            };
            
            double totalRate = 0;

            double criteriaRate = 1;
            for (int i = 0; i < rates.Length; i++)
            {
                if (rates[i].HasValue)
                {
                    totalRate += rates[i].Value * criteriaRate;
                }
                criteriaRate -= (1 / (double)rates.Length);
            }

            return totalRate / rates.Count(r => r.HasValue);
        }

        private double GetCurrentPlayerServeRate()
        {
            return _scoreboard.CurrentServerIndex == 0 ? _p1ServeRate : _p2ServeRate;
        }
    }
}
