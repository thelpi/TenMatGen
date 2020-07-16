using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TenMat;
using TenMat.Data;
using TenMat.Data.Enums;
using TenMat.Sql;
using TenMatGui.ViewModel;

namespace TenMatGui
{
    /// <summary>
    /// Main window interaction logic.
    /// </summary>
    /// <seealso cref="Window"/>
    public partial class MainWindow : Window
    {
        private const int MaxPlayersListed = 50;

        private readonly DateTime _lastRanking = new DateTime(2019, 03, 04);
        private readonly DateTime _firstRanking = new DateTime(1990, 12, 31);
        private readonly List<Player> _players = new List<Player>();
        private readonly SqlMapper sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);
        private readonly IReadOnlyCollection<uint> _drawSizes = new List<uint> { 8, 16, 32, 64, 128 };
        private readonly IReadOnlyCollection<uint> _seedRates = new List<uint> { 2, 4, 8, 16, 32, 0 };
        private readonly BackgroundWorker _bgw;
        private readonly List<PlayerStat> _playerStats = new List<PlayerStat>();

        private DateTime _lastDateLoaded;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            CbbBestOf.ItemsSource = Enum.GetValues(typeof(BestOfEnum));
            CbbFinalBestOf.ItemsSource = Enum.GetValues(typeof(BestOfEnum));
            CbbDrawSize.ItemsSource = _drawSizes;
            CbbFifthSetRule.ItemsSource = Enum.GetValues(typeof(FifthSetTieBreakRuleEnum));
            CbbLevel.ItemsSource = Enum.GetValues(typeof(LevelEnum));
            CbbSurface.ItemsSource = Enum.GetValues(typeof(SurfaceEnum));

            TxtDate.Text = _lastRanking.ToString("yyyy-MM-dd");
            CbbBestOf.SelectedIndex = 1;
            CbbFinalBestOf.SelectedIndex = 1;
            CbbDrawSize.SelectedIndex = 6;
            CbbFifthSetRule.SelectedIndex = 2;
            CbbLevel.SelectedIndex = 0;
            CbbSurface.SelectedIndex = 2;

            _bgw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _bgw.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                _playerStats.Clear();
                PlayerStat.ResetCpt();

                var arg = e.Argument as BgwArg;

                while (!_bgw.CancellationPending)
                {
                    Competition cpt = new Competition(
                    new DrawGenerator(arg.DrawSize, arg.SeedRate == 0 ? 0 : 1 / (double)arg.SeedRate),
                    arg.StartDate,
                    arg.Level,
                    arg.FifthSetTieBreakRule,
                    arg.Surface,
                    arg.Players,
                    arg.BestOf,
                    arg.FinalBestOf,
                    false);

                    while (!cpt.Readonly)
                    {
                        var round = cpt.Draw.Keys.Last();
                        cpt.NextRound();
                    }

                    _bgw.ReportProgress(0, cpt.Draw[RoundEnum.F].First().Winner);
                }
            };
            _bgw.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                var p = e.UserState as Player;

                var matchPl = _playerStats.FirstOrDefault(_ => _.Id == p.Id);
                if (matchPl == null)
                {
                    matchPl = new PlayerStat(p);
                    _playerStats.Add(matchPl);
                }
                matchPl.AddCpt();

                _playerStats.ForEach(_ => _.RefreshCpt());
                _playerStats.Sort();
                LstPlayers.ItemsSource = _playerStats.Take(MaxPlayersListed);
            };
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (_bgw.IsBusy)
            {
                if (!_bgw.CancellationPending)
                {
                    _bgw.CancelAsync();
                }
                return;
            }

            if (!DateTime.TryParse(TxtDate.Text, out DateTime startDate)
                || startDate > _lastRanking
                || startDate < _firstRanking
                || CbbBestOf.SelectedIndex < 0
                || CbbFinalBestOf.SelectedIndex < 0
                || CbbDrawSize.SelectedIndex < 0
                || CbbFifthSetRule.SelectedIndex < 0
                || CbbLevel.SelectedIndex < 0
                || CbbSeedRate.SelectedIndex < 0
                || CbbSurface.SelectedIndex < 0)
            {
                MessageBox.Show("Incomplete or invalid informations.");
                return;
            }

            if (_lastDateLoaded.Date != startDate.Date)
            {
                _players.Clear();
                sqlMap.LoadPlayers((p) => _players.Add(p), startDate, _drawSizes.Max());

                for (int i = 0; i < _drawSizes.Max(); i++)
                {
                    sqlMap.LoadMatches(_players[i], matchesDateMax: startDate);
                }
            }

            var drawSize = (int)((uint)CbbDrawSize.SelectedItem);
            _lastDateLoaded = startDate;

            var seedRate = (uint)CbbSeedRate.SelectedItem;

            _bgw.RunWorkerAsync(new BgwArg
            {
                FifthSetTieBreakRule = (FifthSetTieBreakRuleEnum)CbbFifthSetRule.SelectedItem,
                DrawSize = drawSize,
                StartDate = startDate,
                BestOf = (BestOfEnum)CbbBestOf.SelectedItem,
                FinalBestOf = (BestOfEnum)CbbFinalBestOf.SelectedItem,
                Level = (LevelEnum)CbbLevel.SelectedItem,
                Players = _players.Take(drawSize).ToList(),
                SeedRate = seedRate,
                Surface = (SurfaceEnum)CbbSurface.SelectedItem
            });
        }

        private void BtnRandomize_Click(object sender, RoutedEventArgs e)
        {
            TxtDate.Text = _firstRanking.GetRandomDate(_lastRanking).ToString("yyyy-MM-dd");
            CbbBestOf.SelectedIndex = Tools.Rdm.Next(0, Enum.GetValues(typeof(BestOfEnum)).Length);
            CbbFinalBestOf.SelectedIndex = Tools.Rdm.Next(0, Enum.GetValues(typeof(BestOfEnum)).Length);
            CbbDrawSize.SelectedIndex = Tools.Rdm.Next(0, _drawSizes.Count);
            CbbFifthSetRule.SelectedIndex = Tools.Rdm.Next(0, Enum.GetValues(typeof(FifthSetTieBreakRuleEnum)).Length);
            CbbLevel.SelectedIndex = Tools.Rdm.Next(0, Enum.GetValues(typeof(LevelEnum)).Length);
            CbbSurface.SelectedIndex = Tools.Rdm.Next(0, Enum.GetValues(typeof(SurfaceEnum)).Length);
        }

        private void SetCbbSeedRateFromDrawSize(uint drawSize)
        {
            var seedRatesPickList = _seedRates.Where(sr => sr < drawSize).ToList();
            CbbSeedRate.ItemsSource = seedRatesPickList;
            CbbSeedRate.SelectedIndex = Tools.Rdm.Next(0, seedRatesPickList.Count);
        }

        private void CbbDrawSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbDrawSize.SelectedIndex >= 0)
            {
                SetCbbSeedRateFromDrawSize((uint)CbbDrawSize.SelectedItem);
            }
        }
    }
}
