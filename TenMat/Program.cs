using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data;
using TenMat.Sql;

namespace TenMat
{
    public class Program
    {
        private static List<Player> _players = new List<Player>();

        public static void Main(string[] args)
        {
            SqlMapper sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);

            // new DateTime(1970, 1, 1)
            sqlMap.LoadPlayers((p) => _players.Add(p), null, new DateTime(2019, 03, 04));

            DateTime loadDate = DateTime.Now.AddYears(-5);
            DateTime matchDate = DateTime.Now.AddYears(-1);

            for (int i = 0; i < 128; i++)
            {
                sqlMap.LoadMatches(_players[i], loadDate, true);
            }

            Competition competition = new Competition(new DrawGenerator(128, 0.25), matchDate, LevelEnum.GrandSlam, FifthSetTieBreakRuleEnum.At12_12, SurfaceEnum.Hard, _players);

            while (!competition.Readonly)
            {
                var round = competition.Draw.Keys.Last();
                competition.NextRound();
                LogMatches(competition, round);
                Console.WriteLine(string.Empty);
            }

            Console.ReadLine();
        }

        private static void LogMatches(Competition competition, RoundEnum round)
        {
            Console.WriteLine("=========    " + round.ToString() + "    =========");
            Console.WriteLine(string.Empty);
            foreach (var match in competition.Draw[round])
            {
                Console.WriteLine(match);
            }
            Console.WriteLine(string.Empty);
            Console.WriteLine("===================================");
        }
    }
}
