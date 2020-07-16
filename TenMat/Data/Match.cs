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
        private const double DEF_SERVE_RATE = 6 / (double)10;

        private const double RATE_COEFF_BESTOF = 2 / (double)3;
        private const double RATE_COEFF_LEVEL = 3 / (double)4;
        private const double RATE_COEFF_OPPONENT = 3 / (double)2;
        private const double RATE_COEFF_ROUND = 3 / (double)4;
        private const double RATE_COEFF_SURFACE = 4 / (double)3;
        private const double RATE_COEFF_YEAR = 4 / (double)3;

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
                _p1ServeRate = ComputeSvGameRate(_playerOne);
                _p2ServeRate = ComputeSvGameRate(_playerTwo);
                double p1TieBreakRate = ComputeTieBreakRate(_playerOne);
                double p2TieBreakRate = ComputeTieBreakRate(_playerTwo);
                _p1TieBreakRate = p1TieBreakRate / (p1TieBreakRate + p2TieBreakRate);
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

        private double GetCurrentPlayerServeRate()
        {
            return _scoreboard.IsFirstPlayerAtServe ? _p1ServeRate : _p2ServeRate;
        }

        private double ComputeSvGameRate(Player p)
        {
            uint oppId = p.Id == _playerOne.Id ? _playerTwo.Id : _playerOne.Id;

            var rateCoefficients = new List<double>();
            if (p.SvGameRateByLevel[Level].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByLevel[Level].Value * RATE_COEFF_LEVEL);
            }
            if (p.SvGameRateByRound[Round].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByRound[Round].Value * RATE_COEFF_ROUND);
            }
            if (p.SvGameRateByBestOf[BestOf].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByBestOf[BestOf].Value * RATE_COEFF_BESTOF);
            }
            if (p.SvGameRateByYear[TournamentBeginningDate.Year].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByYear[TournamentBeginningDate.Year].Value * RATE_COEFF_YEAR);
            }
            if (p.SvGameRateByOpponent.ContainsKey(oppId) && p.SvGameRateByOpponent[oppId].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByOpponent[oppId].Value * RATE_COEFF_OPPONENT);
            }
            if (p.SvGameRateBySurface[Surface].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateBySurface[Surface].Value * RATE_COEFF_SURFACE);
            }

            if (rateCoefficients.Count == 0)
            {
                return DEF_SERVE_RATE;
            }

            return rateCoefficients.Sum() / rateCoefficients.Count;
        }

        private double ComputeTieBreakRate(Player p)
        {
            uint oppId = p.Id == _playerOne.Id ? _playerTwo.Id : _playerOne.Id;

            var rateCoefficients = new List<double>();
            if (p.TieBreakRateByLevel[Level].HasValue)
            {
                rateCoefficients.Add(p.TieBreakRateByLevel[Level].Value * RATE_COEFF_LEVEL);
            }
            if (p.TieBreakRateByRound[Round].HasValue)
            {
                rateCoefficients.Add(p.TieBreakRateByRound[Round].Value * RATE_COEFF_ROUND);
            }
            if (p.SvGameRateByBestOf[BestOf].HasValue)
            {
                rateCoefficients.Add(p.SvGameRateByBestOf[BestOf].Value * RATE_COEFF_BESTOF);
            }
            if (p.TieBreakRateByYear[TournamentBeginningDate.Year].HasValue)
            {
                rateCoefficients.Add(p.TieBreakRateByYear[TournamentBeginningDate.Year].Value * RATE_COEFF_YEAR);
            }
            if (p.TieBreakRateByOpponent.ContainsKey(oppId) && p.TieBreakRateByOpponent[oppId].HasValue)
            {
                rateCoefficients.Add(p.TieBreakRateByOpponent[oppId].Value * RATE_COEFF_OPPONENT);
            }
            if (p.TieBreakRateBySurface[Surface].HasValue)
            {
                rateCoefficients.Add(p.TieBreakRateBySurface[Surface].Value * RATE_COEFF_SURFACE);
            }

            if (rateCoefficients.Count == 0)
            {
                return 0.5;
            }

            return rateCoefficients.Sum() / rateCoefficients.Count;
        }
    }
}
