using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    public class Match
    {
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

        public Match(Player p1, Player p2, BestOfEnum bestOf, FifthSetTieBreakRuleEnum fifthSetRule,
            SurfaceEnum surface, LevelEnum level, RoundEnum round, DateTime date)
        {
            if (p1 == null)
            {
                throw new ArgumentNullException(nameof(p1));
            }

            if (p2 == null)
            {
                throw new ArgumentNullException(nameof(p1));
            }

            if (p2.Id == p1.Id)
            {
                throw new ArgumentException("Players should not be the same.", nameof(p2));
            }

            _surface = surface;
            _level = level;
            _round = round;
            _date = date;
            _p2IsFirstToServe = Tools.CoinDraw();
            _playerOne = p1;
            _playerTwo = p2;
            _scoreboard = new Scoreboard(bestOf, _p2IsFirstToServe, fifthSetRule);

            var p1Rate = ComputeRatio(_playerOne, _playerTwo);
            var p2Rate = ComputeRatio(_playerTwo, _playerOne);
            var delta = p1Rate / (p1Rate + p2Rate);

            if (delta > 0.5)
            {
                _p1ServeRatio = DEF_SERVE_RATE + (delta * 0.2);
                _p2ServeRatio = DEF_SERVE_RATE - ((1 - delta) * 0.2);
            }
            else if (delta < 0.5)
            {
                _p1ServeRatio = DEF_SERVE_RATE - (delta * 0.2);
                _p2ServeRatio = DEF_SERVE_RATE + ((1 - delta) * 0.2);
            }
            else
            {
                _p1ServeRatio = DEF_SERVE_RATE;
                _p2ServeRatio = DEF_SERVE_RATE;
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

        public void RunToEnd()
        {
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

        private double GetCurrentPlayerServeRatio()
        {
            if (_scoreboard.CurrentServerIndex == 0)
            {
                return _p1ServeRatio;
            }
            else
            {
                return _p2ServeRatio;
            }
        }

        public override string ToString()
        {
            string line1 = string.Concat(_playerOne.Name, " - ", _playerTwo.Name);
            string line2 = string.Concat(_surface, " - ", _level, " - ", _round);
            string line3 = _scoreboard.ToString();

            return string.Concat(line1, "\r\n", line2, "\r\n", line3);
        }
    }
}
