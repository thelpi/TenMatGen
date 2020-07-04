using System;

namespace TenMat
{
    class Program
    {
        static readonly Random _rdm = new Random();

        static void Main(string[] args)
        {
            MatchScoreboard ms = new MatchScoreboard(3, false);

            while (!ms.IsClosed)
            {
                int rdm = _rdm.Next(1, 11);
                if (rdm <= 3)
                {
                    ms.AddReceiverPoint();
                    Console.WriteLine("Point for receiver ! " + ms.ToString());
                }
                else
                {
                    ms.AddServerPoint();
                    Console.WriteLine("Point for server  !  " + ms.ToString());
                }
                
                System.Threading.Thread.Sleep(200);
            }

            Console.WriteLine("Fin du match !");
            System.Threading.Thread.Sleep(int.MaxValue);
        }
    }
}
