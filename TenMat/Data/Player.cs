using System;
using System.Collections.Generic;
using System.Linq;

namespace TenMat.Data
{
    public class Player
    {
        private static readonly List<Player> _instances = new List<Player>();

        public static bool AddPlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!_instances.Any(p => p.Id == player.Id))
            {
                _instances.Add(player);
                return true;
            }
            return false;
        }

        public static IReadOnlyCollection<Player> Instances { get { return _instances; } }

        private readonly List<MatchHistory> _matchHistoryList = new List<MatchHistory>();

        public uint Id { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public IReadOnlyCollection<MatchHistory> MatchHistoryList
        {
            get
            {
                return _matchHistoryList;
            }
        }

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
        }

        public IEnumerable<MatchHistory> FilterMatchHistoryList(SurfaceEnum? surface = null,
            LevelEnum? level = null,
            RoundEnum? round = null,
            uint? opponentId = null,
            uint? bestOf = null,
            DateTime? dateMin = null,
            DateTime? dateMax = null)
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

        public double GetWinRatio(IEnumerable<MatchHistory> matchHistoryList)
        {
            if (matchHistoryList == null)
            {
                throw new ArgumentNullException(nameof(matchHistoryList));
            }

            return matchHistoryList.Count(mg => mg.WinnerId == Id) / (double)matchHistoryList.Count();
        }

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
    }
}
