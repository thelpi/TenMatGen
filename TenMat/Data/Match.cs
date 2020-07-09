using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a match.
    /// </summary>
    public class Match
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
        private readonly SurfaceEnum _surface;
        private readonly LevelEnum _level;
        private readonly RoundEnum _round;
        private readonly DateTime _date;

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
        /// Constructor.
        /// </summary>
        /// <param name="p1">First player.</param>
        /// <param name="p2">Second player.</param>
        /// <param name="bestOf">Best-of three or five sets.</param>
        /// <param name="fifthSetRule">Tie-break rule for fifth set.</param>
        /// <param name="surface">Surface.</param>
        /// <param name="level">Competition level.</param>
        /// <param name="round">Round.</param>
        /// <param name="date">Match date.</param>
        /// <exception cref="ArgumentNullException"><paramref name="p1"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="p2"/> identifier is the same as <paramref name="p1"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="competition"/> is <c>Null</c>.</exception>
        public Match(Player p1, Player p2, Competition competition, RoundEnum round)
        {
            if (p1 == null)
            {
                throw new ArgumentNullException(nameof(p1));
            }

            if (competition == null)
            {
                throw new ArgumentNullException(nameof(competition));
            }

            if (p2?.Id == p1.Id)
            {
                throw new ArgumentException("Players should not be the same.", nameof(p2));
            }

            _surface = competition.Surface;
            _level = competition.Level;
            _round = round;
            _date = competition.Date;
            _p2IsFirstToServe = Tools.FlipCoin();
            _playerOne = p1;
            _playerTwo = p2;
            if (p2 != null)
            {
                _scoreboard = new Scoreboard(
                    _round == RoundEnum.F ? competition.FinalBestOf : competition.BestOf,
                    _p2IsFirstToServe,
                    competition.FifthSetTieBreakRule);
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
            sb.AppendLine(string.Concat(_surface, " - ", _level, " - ", _round));
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
                var matchesCoeff6 = p.FilterMatchHistoryList(_surface, _level, _round, opp.Id, (uint)_scoreboard.BestOf, _date.AddYears(-(i + 1)), _date.AddYears(-i));
                var matchesCoeff5 = p.FilterMatchHistoryList(_surface, null, _round, opp.Id, (uint)_scoreboard.BestOf, _date.AddYears(-(i + 1)), _date.AddYears(-i))
                    .Except(matchesCoeff6);
                var matchesCoeff4 = p.FilterMatchHistoryList(_surface, null, null, opp.Id, (uint)_scoreboard.BestOf, _date.AddYears(-(i + 1)), _date.AddYears(-i))
                    .Except(matchesCoeff6).Except(matchesCoeff5);
                var matchesCoeff3 = p.FilterMatchHistoryList(_surface, null, null, opp.Id, null, _date.AddYears(-(i + 1)), _date.AddYears(-i))
                    .Except(matchesCoeff6).Except(matchesCoeff5).Except(matchesCoeff4);
                var matchesCoeff2 = p.FilterMatchHistoryList(null, null, null, opp.Id, null, _date.AddYears(-(i + 1)), _date.AddYears(-i))
                    .Except(matchesCoeff6).Except(matchesCoeff5).Except(matchesCoeff4).Except(matchesCoeff3);
                var matchesCoeff1 = p.FilterMatchHistoryList(null, null, null, null, null, _date.AddYears(-(i + 1)), _date.AddYears(-i))
                    .Except(matchesCoeff6).Except(matchesCoeff5).Except(matchesCoeff4).Except(matchesCoeff3).Except(matchesCoeff2);
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

        private static double? WinRatioOnMatchesList(uint playerId, IEnumerable<MatchHistory> matches)
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
