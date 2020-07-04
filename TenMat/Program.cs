﻿using System;

namespace TenMat
{
    public class Program
    {
        private const int _msPause = 5;
        private const double _serveRation = 0.7;
        private const FifthSetTieBreakRuleEnum _fifthSetTieBreakRule = FifthSetTieBreakRuleEnum.None;
        private const bool _p2AtServe = false;

        public static void Main(string[] args)
        {
            MatchScoreboard ms = new MatchScoreboard(_p2AtServe, _fifthSetTieBreakRule);
            SimulateMatch(ms, new Logger(), new Random(), _msPause, _serveRation);
        }

        public static void SimulateMatch(MatchScoreboard ms, ILogger logger,
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

                if (_msPause > 0)
                {
                    System.Threading.Thread.Sleep(_msPause);
                }
            }

            logger.Log("Fin du match !");
            if (_msPause > 0)
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
