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
        private const int MATCH_HISTORY_YEARS = 5;

        private readonly Scoreboard _scoreboard;
        private readonly Player _playerOne;
        private readonly Player _playerTwo;
        private readonly double _p1ServeRatio;
        private readonly double _p2ServeRatio;
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
            if (p2 != null)
            {
                _scoreboard = new Scoreboard(bestOf, _p2IsFirstToServe, fifthSetTieBreakRule, pointByPoint);
                ComputeServeRate(out _p1ServeRatio, out _p2ServeRatio);
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
                if (Tools.Rdm.NextDouble() >= GetCurrentPlayerServeRatio())
                {
                    _scoreboard.AddReceiverPoint();
                }
                else
                {
                    _scoreboard.AddServerPoint();
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
            var matchesByYear = new Dictionary<int, double?[]>();
            for (int i = 0; i < MATCH_HISTORY_YEARS; i++)
            {
                var matchesCoeff1 = p.FilterMatchHistoryList(null, null, null, null, null, TournamentBeginningDate.AddYears(-(i + 1)), TournamentBeginningDate.AddYears(-i));
                var matchesCoeff2 = matchesCoeff1.Where(m => m.LoserId == opp.Id || m.WinnerId == opp.Id);
                var matchesCoeff3 = matchesCoeff2.Where(m => m.Surface == Surface);
                var matchesCoeff4 = matchesCoeff3.Where(m => m.BestOf == _scoreboard.BestOf);
                var matchesCoeff5 = matchesCoeff4.Where(m => m.Round == Round);
                var matchesCoeff6 = matchesCoeff5.Where(m => m.Level == Level);
                matchesByYear.Add(i, new double?[]
                {
                    WinRatioOnMatchesList(p.Id, matchesCoeff6),
                    WinRatioOnMatchesList(p.Id, matchesCoeff5),
                    WinRatioOnMatchesList(p.Id, matchesCoeff4),
                    WinRatioOnMatchesList(p.Id, matchesCoeff3),
                    WinRatioOnMatchesList(p.Id, matchesCoeff2),
                    WinRatioOnMatchesList(p.Id, matchesCoeff1),
                });
            }

            double realValuesCount = 0;
            double totalRate = 0;

            double yearRate = 1;
            foreach (int year in matchesByYear.Keys)
            {
                double criteriaRate = 1;
                int countArgs = matchesByYear[year].Length;
                for (int i = 0; i < countArgs; i++)
                {
                    double currentRate = criteriaRate * yearRate;
                    if (matchesByYear[year][i].HasValue)
                    {
                        totalRate += matchesByYear[year][i].Value * currentRate;
                        realValuesCount++;
                    }
                    criteriaRate -= (1 / (double)countArgs);
                }
                yearRate -= (1 / (double)MATCH_HISTORY_YEARS);
            }

            return totalRate / realValuesCount;
        }

        private static double? WinRatioOnMatchesList(uint playerId, IEnumerable<MatchArchive> matches)
        {
            return matches.Count() == 0 ?
                (double?)null :
                matches.Count(m => m.WinnerId == playerId) / (double)matches.Count();
        }

        private double GetCurrentPlayerServeRatio()
        {
            return _scoreboard.CurrentServerIndex == 0 ? _p1ServeRatio : _p2ServeRatio;
        }
    }
}
