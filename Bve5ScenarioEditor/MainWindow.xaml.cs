using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

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

        #region EventHandler
        /// <summary>
        /// Windowがレンダリングされた後発生するイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_ContentRendered(object sender, EventArgs e)
        {
            LoadScenarios();
        }

        /// <summary>
        /// リストビューの選択されているアイテムが変更された際に発生するイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ScenarioSelectListView_SelectedIndexChanged(object sender, EventArgs e)
        {

            if(scenarioSelectListView.SelectedItems.Count == 0)
            {
                //選択したアイテムがないので情報を非表示に
                thumbnailImage.Visibility = Visibility.Collapsed;
                scenarioTitleText.Visibility = Visibility.Collapsed;
                scenarioCommentText.Visibility = Visibility.Collapsed;
                scenarioRouteTitleText.Visibility = Visibility.Collapsed;
                scenarioRoutePathText.Visibility = Visibility.Collapsed;
                scenarioVehicleTitleText.Visibility = Visibility.Collapsed;
                scenarioVehiclePathText.Visibility = Visibility.Collapsed;
                scenarioAuthorText.Visibility = Visibility.Collapsed;
                scenarioFileNameText.Visibility = Visibility.Collapsed;
            }
            else
            {
                //選択したアイテムの共通項目を調べる
                //ベースとなるアイテムの取得
                Scenario baseScenario = Scenarios.Find(a => a.Item.Equals(scenarioSelectListView.SelectedItems[0]));
                int imgIdx = baseScenario.Item.ImageIndex;
                string title = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.TITLE].Text;
                string routeTitle = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.ROUTE_TITLE].Text;
                string routePath = baseScenario.Data.Route.Count == 0 ? "" : baseScenario.Data.Route[0].Value;
                string vehicleTitle = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.VEHICLE_TITLE].Text;
                string vehiclePath = baseScenario.Data.Vehicle.Count == 0 ? "" : baseScenario.Data.Vehicle[0].Value;
                string author = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.AUTHOR].Text;
                string comment = baseScenario.Data.Comment ?? "";
                string fileName = baseScenario.Item.SubItems[(int)Scenario.SubItemIndex.FILE_NAME].Text;

                //サムネイル情報の描画
                if (imgIdx != -1)
                {
                    Bitmap bitmap = (Bitmap)scenarioSelectListView.LargeImageList.Images[scenarioSelectListView.SelectedItems[0].ImageIndex];
                    using (Stream st = new MemoryStream())
                    {
                        bitmap.Save(st, System.Drawing.Imaging.ImageFormat.Png);
                        st.Seek(0, SeekOrigin.Begin);
                        thumbnailImage.Source = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                //情報をTextBlockに設定
                scenarioTitleText.Text = title;
                scenarioRouteTitleText.Text = routeTitle;
                scenarioRoutePathText.Text = routePath;
                scenarioVehicleTitleText.Text = vehicleTitle;
                scenarioVehiclePathText.Text = vehiclePath;
                scenarioAuthorText.Text = author;
                scenarioCommentText.Text = comment;
                scenarioFileNameText.Text = fileName;

                //初期設定にすべての情報を表示
                thumbnailImage.Visibility = Visibility.Visible;
                scenarioTitleText.Visibility = Visibility.Visible;
                scenarioCommentText.Visibility = Visibility.Visible;
                scenarioRouteTitleText.Visibility = Visibility.Visible;
                scenarioRoutePathText.Visibility = Visibility.Visible;
                scenarioVehicleTitleText.Visibility = Visibility.Visible;
                scenarioVehiclePathText.Visibility = Visibility.Visible;
                scenarioAuthorText.Visibility = Visibility.Visible;
                scenarioFileNameText.Visibility = Visibility.Visible;

                //ベースと異なる情報は非表示に
                foreach (System.Windows.Forms.ListViewItem item in scenarioSelectListView.SelectedItems)
                {
                    Scenario scenario = Scenarios.Find(a => a.Item.Equals(item));

                    if (item.ImageIndex != imgIdx || imgIdx == -1)
                        thumbnailImage.Visibility = Visibility.Collapsed;
                    if (!item.SubItems[(int)Scenario.SubItemIndex.TITLE].Text.Equals(title))
                        scenarioTitleText.Text = "複数タイトル...";
                    if (!item.SubItems[(int)Scenario.SubItemIndex.ROUTE_TITLE].Text.Equals(routeTitle))
                        scenarioRouteTitleText.Text = "複数路線名..."; ;
                    if (scenario.Data.Route.Count == 0)
                        scenarioRoutePathText.Visibility = Visibility.Collapsed;
                    else if(!scenario.Data.Route[0].Value.Equals(routePath))
                        scenarioRoutePathText.Text = "複数路線ファイル...";
                    if (!item.SubItems[(int)Scenario.SubItemIndex.VEHICLE_TITLE].Text.Equals(vehicleTitle))
                        scenarioVehicleTitleText.Text = "複数車両名...";
                    if (scenario.Data.Vehicle.Count == 0)
                        scenarioVehiclePathText.Visibility = Visibility.Collapsed;
                    else if (!scenario.Data.Vehicle[0].Value.Equals(vehiclePath))
                        scenarioVehiclePathText.Text = "複数車両ファイル...";
                    if (!item.SubItems[(int)Scenario.SubItemIndex.AUTHOR].Text.Equals(author))
                        scenarioAuthorText.Text = "複数作者...";
                    if (scenario.Data.Comment == null || scenario.Data.Comment.Equals(""))
                        scenarioCommentText.Visibility = Visibility.Collapsed;
                    else if (!scenario.Data.Comment.Equals(comment))
                        scenarioCommentText.Text = "複数コメント...";
                    if (!item.SubItems[(int)Scenario.SubItemIndex.FILE_NAME].Text.Equals(fileName))
                        scenarioFileNameText.Text = "複数ファイル名...";
                }
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

        #endregion EventHandler

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
            foreach (string file in files)
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

            //シナリオ情報を非表示にする
            thumbnailImage.Visibility = Visibility.Collapsed;
            scenarioTitleText.Visibility = Visibility.Collapsed;
            scenarioCommentText.Visibility = Visibility.Collapsed;
            scenarioRouteTitleText.Visibility = Visibility.Collapsed;
            scenarioRoutePathText.Visibility = Visibility.Collapsed;
            scenarioVehicleTitleText.Visibility = Visibility.Collapsed;
            scenarioVehiclePathText.Visibility = Visibility.Collapsed;
            scenarioAuthorText.Visibility = Visibility.Collapsed;
            scenarioFileNameText.Visibility = Visibility.Collapsed;
        }
    }
}
