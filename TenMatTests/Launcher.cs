using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TenMat;
using TenMat.Data;
using TenMat.Data.Enums;
using TenMat.Sql;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace TenMatTests
{
    [TestClass]
    public class Launcher
    {
        [TestMethod]
        public void TestMethod1()
        {
            int drawSize = 128;

            var date = new DateTime(2019, 03, 04);

            var players = new Dictionary<Player, Dictionary<RoundEnum, int>>();

            var sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);

            sqlMap.LoadPlayers((p) => players.Add(p, Enum.GetValues(typeof(RoundEnum)).Cast<RoundEnum>().ToDictionary(r => r, r => 0)), null, date);

            for (int i = 0; i < drawSize; i++)
            {
                sqlMap.LoadMatches(players.Keys.ElementAt(i), null, true); // date.AddYears(-5)
            }

            var dg = new DrawGenerator(drawSize, 1 / (double)8);

            for (int i = 0; i < 1000; i++)
            {
                var cpt = new Competition(dg, date, LevelEnum.GrandSlam,
                    FifthSetTieBreakRuleEnum.At12_12, SurfaceEnum.Hard,
                    players.Keys, BestOfEnum.Five, BestOfEnum.Five, false);

                while (!cpt.Readonly)
                {
                    cpt.NextRound();
                }
                
                foreach (var round in cpt.Draw.Keys)
                {
                    foreach (var match in cpt.Draw[round])
                    {
                        players[match.Winner][round]++;
                    }
                }
                Debug.WriteLine("current = " + i.ToString());
            }

            using (var sw = new StreamWriter(@"D:\tennisstats.csv"))
            {
                var rowDatas = new List<object> { "name" };
                foreach (var r in players[players.Keys.First()].Keys)
                {
                    rowDatas.Add(r.ToString());
                }
                sw.WriteLine(string.Join("\t", rowDatas));

                foreach (var p in players.Keys)
                {
                    rowDatas = new List<object> { p.Name };
                    foreach (var r in players[p].Keys)
                    {
                        rowDatas.Add(players[p][r]);
                    }
                    sw.WriteLine(string.Join("\t", rowDatas));
                }
            }
        }
    }
}
