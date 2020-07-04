using System;

namespace TenMat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MatchScoreboard ms = new MatchScoreboard(false);
            SimulateMatch(ms, new Logger(), new Random(), true, 0.7);
        }

        public static void SimulateMatch(MatchScoreboard ms, ILogger logger, Random rdm, bool sleep, double serverRatio)
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

                if (sleep)
                {
                    System.Threading.Thread.Sleep(200);
                }
            }

            logger.Log("Fin du match !");
            if (sleep)
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
