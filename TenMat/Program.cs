using System;
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

        public static void Main(string[] args)
        {
            /*SqlMapper sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);

            sqlMap.LoadPlayers(new DateTime(1970, 1, 1));

            var federer = Player.Instances.First(p => p.Name.ToLower().Contains("federer"));

            sqlMap.LoadMatches(federer, DateTime.Now.AddYears(-5));*/

            Scoreboard ms = new Scoreboard(_p2AtServe, _fifthSetTieBreakRule);
            SimulateMatch(ms, new Logger(), new Random(), _msPause, _serveRation);
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

            while (!ms.IsClosed)
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
