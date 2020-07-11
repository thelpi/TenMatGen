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
        /// Win rate by <see cref="SurfaceEnum"/>.
        /// </summary>
        public Dictionary<SurfaceEnum, double?> WinRateBySurface { get; private set; }
        /// <summary>
        /// Win rate by <see cref="LevelEnum"/>.
        /// </summary>
        public Dictionary<LevelEnum, double?> WinRateByLevel { get; private set; }
        /// <summary>
        /// Win rate by opponent identifier.
        /// </summary>
        public Dictionary<uint, double?> WinRateByOpponent { get; private set; }
        /// <summary>
        /// Win rate by year.
        /// </summary>
        public Dictionary<int, double?> WinRateByYear { get; private set; }
        /// <summary>
        /// Win rate by <see cref="BestOfEnum"/>.
        /// </summary>
        public Dictionary<BestOfEnum, double?> WinRateByBestOf { get; private set; }
        /// <summary>
        /// Win rate by <see cref="RoundEnum"/>.
        /// </summary>
        public Dictionary<RoundEnum, double?> WinRateByRound { get; private set; }

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
                matchHistoryList.Where(mh => mh?.WinnerId == Id || mh.LoserId == Id)
            );

            WinRateBySurface = Enum.GetValues(typeof(SurfaceEnum)).Cast<SurfaceEnum>()
                .ToDictionary(s => s, s =>
                    matchHistoryList.Count(m => m.Surface == s) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.Surface == s && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => m.Surface == s))
                    );

            WinRateByRound = Enum.GetValues(typeof(RoundEnum)).Cast<RoundEnum>()
                .ToDictionary(s => s, s =>
                    matchHistoryList.Count(m => m.Round == s) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.Round == s && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => m.Round == s))
                    );

            WinRateByLevel = Enum.GetValues(typeof(LevelEnum)).Cast<LevelEnum>()
                .ToDictionary(s => s, s =>
                    matchHistoryList.Count(m => m.Level == s) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.Level == s && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => m.Level == s))
                    );

            WinRateByBestOf = Enum.GetValues(typeof(BestOfEnum)).Cast<BestOfEnum>()
                .ToDictionary(s => s, s =>
                    matchHistoryList.Count(m => m.BestOf == s) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.BestOf == s && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => m.BestOf == s))
                    );

            WinRateByYear = matchHistoryList.GroupBy(m => m.TournamentBeginningDate.Year)
                .ToDictionary(y => y.Key, y =>
                    matchHistoryList.Count(m => m.TournamentBeginningDate.Year == y.Key) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.TournamentBeginningDate.Year == y.Key && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => m.TournamentBeginningDate.Year == y.Key))
                    );

            WinRateByOpponent = matchHistoryList.GroupBy(m => (m.WinnerId == Id ? m.LoserId : m.WinnerId))
                .ToDictionary(y => y.Key, y =>
                    matchHistoryList.Count(m => (m.WinnerId == Id ? m.LoserId : m.WinnerId) == y.Key) == 0 ? (double?)null : (
                        matchHistoryList.Count(m => m.LoserId == y.Key && m.WinnerId == Id)
                        / (double)matchHistoryList.Count(m => (m.WinnerId == Id ? m.LoserId : m.WinnerId) == y.Key))
                    );

            MatchHistorySet = true;
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
