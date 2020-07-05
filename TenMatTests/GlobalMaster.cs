using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TenMat;

namespace TenMatTests
{
    [TestClass]
    public class GlobalMaster
    {
        [TestMethod]
        public void AssertFullMatch()
        {
            var expectedLogs = GetExpectedLogs();

            var logger = new TestLogger();
            Scoreboard ms = new Scoreboard(false);
            Program.SimulateMatch(ms, logger, new Random(1), 0, 0.7);

            logger.AssertLogs(expectedLogs);
        }

        private static List<string> GetExpectedLogs()
        {
            var expectedLogs = new List<string>();
            string datasFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "Datas", "globalMaster.txt");
            using (var srd = new StreamReader(datasFile))
            {
                while (!srd.EndOfStream)
                {
                    expectedLogs.Add(srd.ReadLine());
                }
            }
            return expectedLogs;
        }
    }

    class TestLogger : ILogger
    {
        private List<string> _logs = new List<string>();

        public IReadOnlyCollection<string> Logs { get { return _logs; } }

        public void Log(string log)
        {
            _logs.Add(log);
        }

        public void AssertLogs(List<string> expected)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(_logs);
            Assert.AreEqual(expected.Count, _logs.Count);
            for (int i = 0; i < expected.Count(); i++)
            {
                Assert.AreEqual(expected[i], _logs[i]);
            }
        }
    }
}
