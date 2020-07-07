using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    /// <summary>
    /// Represents a player.
    /// </summary>
    public class Player
    {
        private readonly List<MatchHistory> _matchHistoryList = new List<MatchHistory>();

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// Full name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        /// <summary>
        /// Indicates if <see cref="MatchHistoryList"/> is set to its final value.
        /// </summary>
        public bool MatchHistorySet { get; private set; }
        /// <summary>
        /// Collection of <see cref="MatchHistory"/>.
        /// </summary>
        public IReadOnlyCollection<MatchHistory> MatchHistoryList
        {
            get
            {
                return _matchHistoryList;
            }
        }

        /// <summary>
        /// Sets <see cref="MatchHistoryList"/> from a collection of matches.
        /// </summary>
        /// <param name="matchHistoryList">Collection of <see cref="MatchHistory"/>.</param>
        public void SetMatchHistoryList(IEnumerable<MatchHistory> matchHistoryList)
        {
            if (matchHistoryList == null)
            {
                throw new ArgumentNullException(nameof(matchHistoryList));
            }

            _matchHistoryList.Clear();
            _matchHistoryList.AddRange(
                matchHistoryList.Where(mh => mh?.WinnerId == Id || mh.LoserId == Id)
            );
            MatchHistorySet = true;
        }

        /// <summary>
        /// Filters matches from <see cref="MatchHistoryList"/> by criteria.
        /// </summary>
        /// <param name="surface">Optionnal; court surface.</param>
        /// <param name="level">Optionnal; level of competition.</param>
        /// <param name="round">Optionnal; round of competition.</param>
        /// <param name="opponentId">Optionnal; opponent identifier.</param>
        /// <param name="bestOf">Optionnal; best of three or five sets.</param>
        /// <param name="dateMin">Optionnal; minimal date of the match.</param>
        /// <param name="dateMax">Optionnal; maximal date of the match.</param>
        /// <returns></returns>
        public IEnumerable<MatchHistory> FilterMatchHistoryList(SurfaceEnum? surface = null,
            LevelEnum? level = null, RoundEnum? round = null, uint? opponentId = null,
            uint? bestOf = null, DateTime? dateMin = null, DateTime? dateMax = null)
        {
            var matches = MatchHistoryList.AsEnumerable();
            if (surface.HasValue)
            {
                matches = matches.Where(mg => mg.Surface == surface.Value);
            }
            if (level.HasValue)
            {
                matches = matches.Where(mg => mg.Level == level.Value);
            }
            if (round.HasValue)
            {
                matches = matches.Where(mg => mg.Round == round.Value);
            }
            if (opponentId.HasValue && opponentId.Value != Id)
            {
                matches = matches.Where(mg => mg.WinnerId == opponentId.Value || mg.LoserId == opponentId.Value);
            }
            if (bestOf.HasValue)
            {
                matches = matches.Where(mg => mg.BestOf == bestOf.Value);
            }
            if (dateMin.HasValue)
            {
                matches = matches.Where(mg => mg.Date.Date >= dateMin.Value.Date);
            }
            if (dateMax.HasValue)
            {
                matches = matches.Where(mg => mg.Date.Date <= dateMax.Value.Date);
            }
            return matches;
        }

        /// <summary>
        /// Tool to get the fullname of a player for firstname and lastname.
        /// </summary>
        /// <param name="firstName">First name.</param>
        /// <param name="lastName">Last name.</param>
        /// <returns>Full name.</returns>
        public static string GetFullName(string firstName, string lastName)
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
