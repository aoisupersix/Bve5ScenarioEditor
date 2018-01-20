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
        /// シナリオに設定されたサムネイルの大きさ
        /// </summary>
        public static System.Drawing.Size ThumbnailSize { get; private set; }

        /// <summary>
        /// シナリオの再グルーピングを行います。
        /// </summary>
        /// <param name="subIdx">グルーピングする項目</param>
        void ReGroupingFor(Scenario.SubItemIndex subIdx)
        {
            scenarioSelectListView.Groups.Clear();
            foreach(Scenario scenario in Scenarios)
            {
                scenario.AddGroup(scenarioSelectListView, (int)subIdx);
            }
        }

        #region 表示メニューのクリックイベント

        /// <summary>
        /// 表示切り替えメニューのアイテムを排他チェックします。
        /// </summary>
        /// <param name="checkItem">チェックするメニューアイテム</param>
        void CheckDisplayMenuItem(System.Windows.Controls.MenuItem checkItem)
        {
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
            CheckDisplayMenuItem((System.Windows.Controls.MenuItem)sender);
            scenarioSelectListView.View = View.LargeIcon;
        }

        /// <summary>
        /// ScenarioSelectListViewの表示方法を詳細に切り替えます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Display_Details(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem((System.Windows.Controls.MenuItem)sender);
            scenarioSelectListView.View = View.Details;
        }

        /// <summary>
        /// ScenarioSelectListViewの表示方法を並べて表示に切り替えます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Display_Tile(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem((System.Windows.Controls.MenuItem)sender);
            scenarioSelectListView.View = View.Tile;
        }

        void Sort_Title(object sender, RoutedEventArgs e)
        {

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
                    scenario.CreateListViewItem(scenarioSelectListView);
                }
            }

            foreach (var group in scenarioSelectListView.Groups)
            {
                Console.WriteLine("group:.ToString -> {0}", group.ToString());
            }
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
