using System;
using System.Collections.Generic;
using System.Linq;
using TenMat.Data;
using TenMat.Sql;

namespace TenMat
{
    public class Program
    {
        private const int _msPause = 50;
        private const double _serveRation = 0.8;
        private const FifthSetTieBreakRuleEnum _fifthSetTieBreakRule = FifthSetTieBreakRuleEnum.At12_12;
        private const bool _p2AtServe = false;

        private static List<Player> _players = new List<Player>();

        public static void Main(string[] args)
        {
            SqlMapper sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);

            // new DateTime(1970, 1, 1)
            sqlMap.LoadPlayers((p) => _players.Add(p), null, new DateTime(2019, 03, 04));

            DateTime loadDate = DateTime.Now.AddYears(-5);
            DateTime matchDate = DateTime.Now.AddYears(-1);

            Competition competition = new Competition(new DrawGenerator(128, 0.25), matchDate, LevelEnum.GrandSlam, FifthSetTieBreakRuleEnum.At12_12, SurfaceEnum.Hard, _players);
           /* while (true)
            {
                Player p1 = _players[Tools.Rdm.Next(0, _players.Count)];
                Player p2 = _players[Tools.Rdm.Next(0, _players.Count)];
                while (p2.Id == p1.Id)
                {
                    p2 = _players[Tools.Rdm.Next(0, _players.Count)];
                }
                LevelEnum level = (LevelEnum)Tools.Rdm.Next(1, 8);
                SurfaceEnum surface = (SurfaceEnum)Tools.Rdm.Next(1, 4);
                RoundEnum round = (RoundEnum)Tools.Rdm.Next(1, 10);
                BestOfEnum bestOf = Tools.Rdm.Next(0, 2) == 1 ? BestOfEnum.Five : BestOfEnum.Three;

                sqlMap.LoadMatches(p1, loadDate, true);
                sqlMap.LoadMatches(p2, loadDate, true);

                Match m = new Match(p1, p2, bestOf, FifthSetTieBreakRuleEnum.At12_12,
                    surface, level, round, matchDate);

                m.RunToEnd();

                Console.WriteLine(m.ToString());
                System.Threading.Thread.Sleep(200);
            }*/

            /*Scoreboard ms = new Scoreboard(bestOf, _p2AtServe, _fifthSetTieBreakRule);
            SimulateMatch(ms, new Logger(), new Random(), _msPause, _serveRation);*/
        }

        public static void SimulateMatch(Scoreboard ms, ILogger logger,
            Random rdm, int pause, double serverRatio)
        {
            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (rdm == null)
            {
                throw new ArgumentNullException(nameof(rdm));
            }

            if (serverRatio <= 0 || serverRatio >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(serverRatio), serverRatio, "The parameter should be greater than zero and lower than one.");
            }

            while (!ms.Readonly)
            {
                double rdmValue = rdm.NextDouble();
                if (rdmValue >= serverRatio)
                {
                    ms.AddReceiverPoint();
                    logger.Log("Point for receiver ! " + ms.ToString());
                }
                else
                {
                    ms.AddServerPoint();
                    logger.Log("Point for server  !  " + ms.ToString());
                }

                if (pause > 0)
                {
                    System.Threading.Thread.Sleep(pause);
                }
            }

            logger.Log("Fin du match !");
            if (pause > 0)
            {
                System.Threading.Thread.Sleep(int.MaxValue);
            }
        }
    }

    public interface ILogger
    {
        void Log(string log);
    }

    public class Logger : ILogger
    {
        public void Log(string log)
        {
            Console.WriteLine(log);
        }
    }
}
