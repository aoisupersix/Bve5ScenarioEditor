using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        string dirPath = @"F:\Library\Documents\Bvets\Scenarios"; //TODO

        /// <summary>
        /// 各シナリオ
        /// </summary>
        List<Scenario> Scenarios = new List<Scenario>();

        /// <summary>
        /// シナリオをグルーピングする項目
        /// </summary>
        Scenario.SubItemIndex defaultGroupIdx = Scenario.SubItemIndex.ROUTE_TITLE;

        /// <summary>
        /// シナリオに設定されたサムネイルの大きさ
        /// </summary>
        public static System.Drawing.Size ThumbnailSize { get; private set; }

        /// <summary>
        /// シナリオのグルーピングとソートを行います。
        /// </summary>
        /// <param name="subIdx">グルーピングする項目</param>
        void GroupingFor(Scenario.SubItemIndex subIdx)
        {
            //ソート
            Scenarios.Sort((a, b) => string.Compare(a.Item.SubItems[(int)subIdx].Text, b.Item.SubItems[(int)subIdx].Text));

            scenarioSelectListView.Items.Clear();
            scenarioSelectListView.Groups.Clear();

            //グルーピング
            foreach(Scenario scenario in Scenarios)
            {
                scenarioSelectListView.Items.Add(scenario.Item);
                scenario.AddGroup(scenarioSelectListView, (int)subIdx);
            }
        }

        #region 表示メニューのクリックイベント

        /// <summary>
        /// 表示切り替えメニューのアイテムを排他チェックします。
        /// </summary>
        /// <param name="displayMenuItem">チェックするメニューアイテム</param>
        void CheckDisplayMenuItem(object displayMenuItem)
        {
            System.Windows.Controls.MenuItem checkItem = (System.Windows.Controls.MenuItem)displayMenuItem;
            checkItem.IsChecked = true;

            //checkItem以外のMenuItemのチェックを外す
            foreach (System.Windows.Controls.MenuItem item in menuItem_Display.Items)
            {
                if (checkItem != item)
                    item.IsChecked = false;
            }
        }

        /// <summary>
        /// ScenarioSelectListViewの表示方法をアイコンに切り替えます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Display_Icon(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = View.LargeIcon;
        }

        /// <summary>
        /// ScenarioSelectListViewの表示方法を詳細に切り替えます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Display_Details(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = View.Details;
        }

        /// <summary>
        /// ScenarioSelectListViewの表示方法を並べて表示に切り替えます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Display_Tile(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = View.Tile;
        }

        /// <summary>
        /// ソートメニューのアイテムを排他チェックします。
        /// </summary>
        /// <param name="checkItem">チェックするメニューアイテム</param>
        void CheckSortMenuItem(object sortMenuItem)
        {
            System.Windows.Controls.MenuItem checkItem = (System.Windows.Controls.MenuItem)sortMenuItem;
            checkItem.IsChecked = true;

            //checkItem以外のMenuItemのチェックを外す
            foreach (System.Windows.Controls.MenuItem item in menuItem_Sort.Items)
            {
                if (checkItem != item)
                    item.IsChecked = false;
            }
        }

        void Sort_Title(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            GroupingFor(Scenario.SubItemIndex.TITLE);
        }

        void Sort_RouteTitle(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            GroupingFor(Scenario.SubItemIndex.ROUTE_TITLE);
        }

        void Sort_VehicleTitle(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            GroupingFor(Scenario.SubItemIndex.VEHICLE_TITLE);
        }

        void Sort_Author(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            GroupingFor(Scenario.SubItemIndex.AUTHOR);
        }

        void Sort_File(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            GroupingFor(Scenario.SubItemIndex.FILE_NAME);
        }

        #endregion 表示メニューのクリックイベント

        /// <summary>
        /// 現在のディレクトリにあるシナリオファイルを読み込みます。
        /// </summary>
        void LoadScenarios()
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = ThumbnailSize;
            scenarioSelectListView.LargeImageList = imgList;

            //とりあえずパスの追加 TODO: あとで修正
            filePathComboBox.Items.Add(dirPath);
            this.filePathComboBox.SelectedIndex = this.filePathComboBox.Items.Count - 1;

            //シナリオの読み込み
            string[] files = Directory.GetFiles(dirPath, "*");
            foreach(string file in files)
            {
                Scenario scenario = new Scenario(file);
                if (scenario.Load())
                {
                    Scenarios.Add(scenario);
                    scenarioSelectListView.Items.Add(scenario.CreateListViewItem(scenarioSelectListView));
                }
            }

            GroupingFor(defaultGroupIdx);
        }

        public MainWindow()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            InitializeComponent();

            ThumbnailSize = new System.Drawing.Size(96, 96);
            scenarioSelectListView.View = View.LargeIcon;
            LoadScenarios();
        }
    }
}
