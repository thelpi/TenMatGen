using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TenMat;
using TenMat.Data;
using TenMat.Data.Enums;
using TenMat.Sql;

namespace TenMatGui
{
    /// <summary>
    /// Main window interaction logic.
    /// </summary>
    /// <seealso cref="Window"/>
    public partial class MainWindow : Window
    {
        private readonly DateTime _lastRanking = new DateTime(2019, 03, 04);
        private readonly DateTime _firstRanking = new DateTime(1968, 01, 29);
        private readonly List<Player> _players = new List<Player>();
        private readonly SqlMapper sqlMap = new SqlMapper("localhost", "nice_tennis_denis", "root", null);

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            CbbBestOf.ItemsSource = Enum.GetValues(typeof(BestOfEnum));
            CbbFinalBestOf.ItemsSource = Enum.GetValues(typeof(BestOfEnum));
            CbbDrawSize.ItemsSource = new List<int> { 2, 4, 8, 16, 32, 64, 128 };
            CbbFifthSetRule.ItemsSource = Enum.GetValues(typeof(FifthSetTieBreakRuleEnum));
            CbbLevel.ItemsSource = Enum.GetValues(typeof(LevelEnum));
            CbbSeedRate.ItemsSource = new List<double> { 0.5, 0.25, 0.125, 0.0625, 0.03125, 0 };
            CbbSurface.ItemsSource = Enum.GetValues(typeof(SurfaceEnum));

            TxtDate.Text = _lastRanking.ToString("yyyy-MM-dd");
            CbbBestOf.SelectedIndex = 1;
            CbbFinalBestOf.SelectedIndex = 1;
            CbbDrawSize.SelectedIndex = 6;
            CbbFifthSetRule.SelectedIndex = 2;
            CbbLevel.SelectedIndex = 0;
            CbbSeedRate.SelectedIndex = 2;
            CbbSurface.SelectedIndex = 2;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
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

            GrdMain.ColumnDefinitions.Clear();
            GrdMain.Children.Clear();

            int drawSize = (int)CbbDrawSize.SelectedItem;

            _players.Clear();
            sqlMap.LoadPlayers((p) => _players.Add(p), null, startDate);

            for (int i = 0; i < drawSize; i++)
            {
                sqlMap.LoadMatches(_players[i], null, true); // startDate.AddYears(-5)
            }

            Competition cpt = new Competition(
                new DrawGenerator(drawSize, (double)CbbSeedRate.SelectedItem),
                startDate,
                (LevelEnum)CbbLevel.SelectedItem,
                (FifthSetTieBreakRuleEnum)CbbFifthSetRule.SelectedItem,
                (SurfaceEnum)CbbSurface.SelectedItem,
                _players.Take(drawSize),
                (BestOfEnum)CbbBestOf.SelectedItem,
                (BestOfEnum)CbbFinalBestOf.SelectedItem,
                false);

            while (!cpt.Readonly)
            {
                var round = cpt.Draw.Keys.Last();
                cpt.NextRound();
                LogMatches(cpt, round);
            }
        }

        private void LogMatches(Competition competition, RoundEnum round)
        {
            GrdMain.ColumnDefinitions.Add(new ColumnDefinition());

            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            sp.SetValue(Grid.ColumnProperty, GrdMain.ColumnDefinitions.Count - 1);
            sp.SetValue(Grid.RowProperty, 0);

            Label lbl = new Label { Content = round.ToString() };
            sp.Children.Add(lbl);

            foreach (var match in competition.Draw[round])
            {
                lbl = new Label { Content = match.ToString() };
                sp.Children.Add(lbl);
            }

            GrdMain.Children.Add(sp);
        }
    }
}
