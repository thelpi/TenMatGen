using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data.Enums;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a player.
    /// </summary>
    public class Player
    {
        private readonly List<MatchArchive> _matchHistoryList = new List<MatchArchive>();

        private readonly Dictionary<SurfaceEnum, double?> _svGameRateBySurface
            = new Dictionary<SurfaceEnum, double?>();
        private readonly Dictionary<LevelEnum, double?> _svGameRateByLevel
            = new Dictionary<LevelEnum, double?>();
        private readonly Dictionary<uint, double?> _svGameRateByOpponent
            = new Dictionary<uint, double?>();
        private readonly Dictionary<int, double?> _svGameRateByYear
            = new Dictionary<int, double?>();
        private readonly Dictionary<BestOfEnum, double?> _svGameRateByBestOf
            = new Dictionary<BestOfEnum, double?>();
        private readonly Dictionary<RoundEnum, double?> _svGameRateByRound
            = new Dictionary<RoundEnum, double?>();

        private readonly Dictionary<SurfaceEnum, double?> _tieBreakRateBySurface
            = new Dictionary<SurfaceEnum, double?>();
        private readonly Dictionary<LevelEnum, double?> _tieBreakRateByLevel
            = new Dictionary<LevelEnum, double?>();
        private readonly Dictionary<uint, double?> _tieBreakRateByOpponent
            = new Dictionary<uint, double?>();
        private readonly Dictionary<int, double?> _tieBreakRateByYear
            = new Dictionary<int, double?>();
        private readonly Dictionary<BestOfEnum, double?> _tieBreakRateByBestOf
            = new Dictionary<BestOfEnum, double?>();
        private readonly Dictionary<RoundEnum, double?> _tieBreakRateByRound
            = new Dictionary<RoundEnum, double?>();

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public uint Id { get; }
        /// <summary>
        /// Full name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateTime? DateOfBirth { get; }
        /// <summary>
        /// Indicates if <see cref="MatchHistoryList"/> is set to its final value.
        /// </summary>
        public bool MatchHistorySet { get; private set; }
        /// <summary>
        /// Collection of <see cref="MatchArchive"/>.
        /// </summary>
        public IReadOnlyCollection<MatchArchive> MatchHistoryList
        {
            get
            {
                return _matchHistoryList;
            }
        }

        /// <summary>
        /// Games win rate by <see cref="SurfaceEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<SurfaceEnum, double?> SvGameRateBySurface { get { return _svGameRateBySurface; } }
        /// <summary>
        /// Games win rate by <see cref="LevelEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<LevelEnum, double?> SvGameRateByLevel { get { return _svGameRateByLevel; } }
        /// <summary>
        /// Games win rate by opponent identifier.
        /// </summary>
        public IReadOnlyDictionary<uint, double?> SvGameRateByOpponent { get { return _svGameRateByOpponent; } }
        /// <summary>
        /// Games win rate by year.
        /// </summary>
        public IReadOnlyDictionary<int, double?> SvGameRateByYear { get { return _svGameRateByYear; } }
        /// <summary>
        /// Games win rate by <see cref="BestOfEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<BestOfEnum, double?> SvGameRateByBestOf { get { return _svGameRateByBestOf; } }
        /// <summary>
        /// Games win rate by <see cref="RoundEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<RoundEnum, double?> SvGameRateByRound { get { return _svGameRateByRound; } }

        /// <summary>
        /// Tie-breaks win rate by <see cref="SurfaceEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<SurfaceEnum, double?> TieBreakRateBySurface { get { return _tieBreakRateBySurface; } }
        /// <summary>
        /// Tie-breaks win rate by <see cref="LevelEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<LevelEnum, double?> TieBreakRateByLevel { get { return _tieBreakRateByLevel; } }
        /// <summary>
        /// Tie-breaks win rate by opponent identifier.
        /// </summary>
        public IReadOnlyDictionary<uint, double?> TieBreakRateByOpponent { get { return _tieBreakRateByOpponent; } }
        /// <summary>
        /// Tie-breaks win rate by year.
        /// </summary>
        public IReadOnlyDictionary<int, double?> TieBreakRateByYear { get { return _tieBreakRateByYear; } }
        /// <summary>
        /// Tie-breaks win rate by <see cref="BestOfEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<BestOfEnum, double?> TieBreakRateByBestOf { get { return _tieBreakRateByBestOf; } }
        /// <summary>
        /// Tie-breaks win rate by <see cref="RoundEnum"/>.
        /// </summary>
        public IReadOnlyDictionary<RoundEnum, double?> TieBreakRateByRound { get { return _tieBreakRateByRound; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The <see cref="Id"/> value.</param>
        /// <param name="firstName">First name.</param>
        /// <param name="lastName">Last name.</param>
        /// <param name="dateOfBirth">The <see cref="DateOfBirth"/> value.</param>
        public Player(uint id, string firstName, string lastName, DateTime? dateOfBirth)
        {
            Id = id;
            Name = GetFullName(firstName, lastName);
            DateOfBirth = dateOfBirth;
        }

        /// <summary>
        /// Sets <see cref="MatchHistoryList"/> from a collection of matches.
        /// </summary>
        /// <param name="matchHistoryList">Collection of <see cref="MatchArchive"/>.</param>
        public void SetMatchHistoryList(IEnumerable<MatchArchive> matchHistoryList)
        {
            if (matchHistoryList == null)
            {
                throw new ArgumentNullException(nameof(matchHistoryList));
            }

            _matchHistoryList.Clear();
            _matchHistoryList.AddRange(
                // redundant with the database query; just to be safe.
                matchHistoryList.Where(mh => mh?.WinnerId == Id || mh.LoserId == Id)
            );

            ComputeGameRateByCriterion(
                _svGameRateBySurface, _tieBreakRateBySurface,
                Enum.GetValues(typeof(SurfaceEnum)).Cast<SurfaceEnum>(),
                s => _matchHistoryList.Where(m => m.Surface == s)
            );
            ComputeGameRateByCriterion(
                _svGameRateByLevel, _tieBreakRateByLevel,
                Enum.GetValues(typeof(LevelEnum)).Cast<LevelEnum>(),
                l => _matchHistoryList.Where(m => m.Level == l)
            );
            ComputeGameRateByCriterion(
                _svGameRateByOpponent, _tieBreakRateByOpponent,
                _matchHistoryList.Select(m => m.WinnerId == Id ? m.LoserId : m.WinnerId).Distinct(),
                oId => _matchHistoryList.Where(m => (m.WinnerId == Id ? m.LoserId : m.WinnerId) == oId)
            );
            ComputeGameRateByCriterion(
                _svGameRateByYear, _tieBreakRateByYear,
                Enumerable.Range(Tools.OPEN_ERA_YEAR, DateTime.Now.Year - Tools.OPEN_ERA_YEAR),
                y => _matchHistoryList.Where(m => m.TournamentBeginningDate.Year == y)
            );
            ComputeGameRateByCriterion(
                _svGameRateByBestOf, _tieBreakRateByBestOf,
                Enum.GetValues(typeof(BestOfEnum)).Cast<BestOfEnum>(),
                b => _matchHistoryList.Where(m => m.BestOf == b)
            );
            ComputeGameRateByCriterion(
                _svGameRateByRound, _tieBreakRateByRound,
                Enum.GetValues(typeof(RoundEnum)).Cast<RoundEnum>(),
                r => _matchHistoryList.Where(m => m.Round == r)
            );

            MatchHistorySet = true;
        }

        private void ComputeGameRateByCriterion<T>(
            Dictionary<T, double?> gamesDictionary,
            Dictionary<T, double?> tieBreaksDictionary,
            IEnumerable<T> keys,
            Func<T, IEnumerable<MatchArchive>> matchesFilter)
        {
            gamesDictionary.Clear();
            tieBreaksDictionary.Clear();
            foreach (T key in keys)
            {
                int gamesCount = matchesFilter(key).Sum(m => m.Sets.Sum(s => s.GamesCount));
                int gamesCountAsWinner = matchesFilter(key).Where(m => m.WinnerId == Id).Sum(m => m.Sets.Sum(s => s.Games(0)));
                int gamesCountAsLoser = matchesFilter(key).Where(m => m.LoserId == Id).Sum(m => m.Sets.Sum(s => s.Games(1)));
                gamesDictionary.Add(key, gamesCount == 0 ? (double?)null : (gamesCountAsWinner + gamesCountAsLoser) / (double)gamesCount);

                int tieBreaksCount = matchesFilter(key).Sum(m => m.Sets.Count(s => s.HasTieBreak));
                int tieBreaksCountAsWinner = matchesFilter(key).Where(m => m.WinnerId == Id).Sum(m => m.Sets.Count(s => s.HasTieBreak && s.IsWonBy(0)));
                int tieBreaksCountAsLoser = matchesFilter(key).Where(m => m.LoserId == Id).Sum(m => m.Sets.Count(s => s.HasTieBreak && s.IsWonBy(1)));
                tieBreaksDictionary.Add(key, tieBreaksCount == 0 ? (double?)null : (tieBreaksCountAsWinner + tieBreaksCountAsLoser) / (double)tieBreaksCount);
            }
        }

        private string GetFullName(string firstName, string lastName)
        {
            string fullName = string.Empty;

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                fullName = lastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (fullName == string.Empty)
                {
                    fullName = firstName.Trim();
                }
                else
                {
                    fullName = string.Concat(fullName, ", ", firstName.Trim());
                }
            }

            return fullName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
