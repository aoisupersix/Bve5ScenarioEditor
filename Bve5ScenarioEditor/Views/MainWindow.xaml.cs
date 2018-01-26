using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using Wf = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using MahApps.Metro.Controls;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// 現在のディレクトリパス
        /// </summary>
        string dirPath;

        /// <summary>
        /// シナリオをグルーピング(ソート)する項目
        /// </summary>
        Scenario.SubItemIndex groupIdx = Scenario.SubItemIndex.ROUTE_TITLE;

        /// <summary>
        /// シナリオ情報の管理クラス
        /// </summary>
        ScenarioDataManagement scenarioManager;

        //以下コンテキストメニューのアイテム
        Wf.MenuItem contextMenuItem_Edit = new Wf.MenuItem("シナリオを編集(&E)");
        Wf.MenuItem contextMenuItem_Delete = new Wf.MenuItem("シナリオを削除(&D)");

        /// <summary>
        /// シナリオに設定されたサムネイルの大きさ
        /// </summary>
        public static System.Drawing.Size ThumbnailSize { get; private set; }

        /// <summary>
        /// 現在メッセージ待ち行列の中にある全てのUIメッセージを処理します。
        /// </summary>
        void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// コンテキストメニューの初期化をします。
        /// </summary>
        void InitializeContextMenu()
        {
            contextMenuItem_Edit.Click += EditSelectedScenario;

            contextMenu.MenuItems.Add(contextMenuItem_Edit);
            contextMenu.MenuItems.Add(contextMenuItem_Delete);
        }

        /// <summary>
        /// シナリオ情報の編集ウインドウを表示します。
        /// </summary>
        void ShowEditWindow()
        {
            if (scenarioSelectListView.SelectedItems.Count > 0)
            {
                List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
                List<Scenario> editData = new List<Scenario>();
                foreach (Wf.ListViewItem item in scenarioSelectListView.SelectedItems)
                {
                    editData.Add(scenarios.Find(a => a.Item.Equals(item)));
                }
                EditWindow editWindow = new EditWindow();
                editWindow.Owner = this;
                Scenario[] returnData = editWindow.ShowWindow(editData.ToArray());
                foreach (var ret in returnData)
                {
                    ret.CreateListViewItem(scenarioSelectListView);
                    int idx = scenarios.FindIndex(x => x.File.Equals(ret.File));
                    scenarios[idx] = ret;
                }
                scenarioManager.SetNewMemento(scenarios);
                GroupingFor(groupIdx);
            }
        }

        /// <summary>
        /// シナリオのグルーピングとソートを行います。
        /// </summary>
        /// <param name="subIdx">グルーピングする項目</param>
        void GroupingFor(Scenario.SubItemIndex subIdx)
        {
            //ソート
            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            scenarios.Sort((a, b) => string.Compare(a.Item.SubItems[(int)subIdx].Text, b.Item.SubItems[(int)subIdx].Text));

            scenarioSelectListView.Items.Clear();
            scenarioSelectListView.Groups.Clear();

            //グルーピング
            foreach(Scenario scenario in scenarios)
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

        /// <summary>
        /// シナリオ情報表示をすべて非表示にします。
        /// </summary>
        void ClearScenarioInfo()
        {
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

        #region EventHandler
        /// <summary>
        /// Windowがレンダリングされた後、コンボボックスにファイルパスを追加します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Window_ContentRendered(object sender, EventArgs e)
        {
            //ファイルパスを追加
            filePathComboBox.Items.Add(dirPath);
            this.filePathComboBox.SelectedIndex = this.filePathComboBox.Items.Count - 1;
            //コンボボックスのイベントからシナリオを読み込む
        }

        /// <summary>
        /// リストビューの選択されているアイテムが変更された際に、シナリオ情報を更新します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ScenarioSelectListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            statusText.Text = "シナリオを" + scenarioSelectListView.SelectedItems.Count + "個選択中。";

            if (scenarioSelectListView.SelectedItems.Count == 0)
            {
                //選択したアイテムがないので情報を非表示に
                ClearScenarioInfo();
            }
            else
            {
                //選択したアイテムの共通項目を調べる
                //ベースとなるアイテムの取得
                Scenario baseScenario = scenarios.Find(a => a.Item.Equals(scenarioSelectListView.SelectedItems[0]));
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
                foreach (Wf.ListViewItem item in scenarioSelectListView.SelectedItems)
                {
                    Scenario scenario = scenarios.Find(a => a.Item.Equals(item));

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
        /// EditWindowを表示し、選択しているシナリオを編集します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void EditSelectedScenario(object sender, EventArgs e)
        {
            ShowEditWindow();
        }

        /// <summary>
        /// EditWindowを表示し、選択しているシナリオを編集します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void EditSelectedScenario(object sender, RoutedEventArgs e)
        {
            ShowEditWindow();
        }

        /// <summary>
        /// コンテキストメニューが表示される際、どのアイテムを表示するかを決めます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ContextMenu_PopUp(object sender, EventArgs e)
        {
            System.Drawing.Point point = scenarioSelectListView.PointToClient(Wf.Cursor.Position);
            Wf.ListViewItem item = scenarioSelectListView.HitTest(point).Item;
            if (item != null && item.Bounds.Contains(point))
            {
                //マウスがアイテムの範囲内にあるのでメニューを有効化
                contextMenuItem_Edit.Visible = true;
                contextMenuItem_Delete.Visible = true;
            }
            else
            {
                //メニューを無効化
                contextMenuItem_Edit.Visible = false;
                contextMenuItem_Delete.Visible = false;
            }
        }

        /// <summary>
        /// 参照ボタンをクリックした際、ディレクトリを選択するダイアログを表示します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wf.FolderBrowserDialog();
            if(dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                dirPath = dlg.SelectedPath;
                filePathComboBox.Items.Add(dirPath);
                this.filePathComboBox.SelectedIndex = this.filePathComboBox.Items.Count - 1;
            }
        }

        /// <summary>
        /// コンボボックスのアイテムが変更された際、シナリオを読み込みます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void FilePathComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            dirPath = (string)filePathComboBox.SelectedValue;
            LoadScenarios();
        }

        /// <summary>
        /// リストビューの表示方法をアイコンに切り替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Display_Icon(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = Wf.View.LargeIcon;
        }

        /// <summary>
        /// リストビューの表示方法を詳細に切り替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Display_Details(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = Wf.View.Details;
        }

        /// <summary>
        /// リストビューの表示方法を並べて表示に切り替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Display_Tile(object sender, RoutedEventArgs e)
        {
            CheckDisplayMenuItem(sender);
            scenarioSelectListView.View = Wf.View.Tile;
        }

        /// <summary>
        /// タイトルでシナリオを並び替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Sort_Title(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            groupIdx = Scenario.SubItemIndex.TITLE;
            GroupingFor(groupIdx);
        }

        /// <summary>
        /// 路線名でシナリオを並び替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Sort_RouteTitle(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            groupIdx = Scenario.SubItemIndex.ROUTE_TITLE;
            GroupingFor(groupIdx);
        }

        /// <summary>
        /// 車両名でシナリオを並び替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Sort_VehicleTitle(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            groupIdx = Scenario.SubItemIndex.VEHICLE_TITLE;
            GroupingFor(groupIdx);
        }

        /// <summary>
        /// 作者名でシナリオを並び替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Sort_Author(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            groupIdx = Scenario.SubItemIndex.AUTHOR;
            GroupingFor(groupIdx);
        }

        /// <summary>
        /// ファイル名でシナリオを並び替えます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Sort_File(object sender, RoutedEventArgs e)
        {
            CheckSortMenuItem(sender);
            groupIdx = Scenario.SubItemIndex.FILE_NAME;
            GroupingFor(groupIdx);
        }

        /// <summary>
        /// VersionWindowを表示します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Show_VersionWindow(object sender, RoutedEventArgs e)
        {
            VersionWindow verWindow = new VersionWindow();
            verWindow.Owner = this;
            verWindow.ShowDialog();
        }

        /// <summary>
        /// ウインドウを閉じます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Exit_Program(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion EventHandler

        /// <summary>
        /// 現在のディレクトリにあるシナリオファイルを読み込みます。
        /// </summary>
        void LoadScenarios()
        {
            statusText.Text = "シナリオの読み込み中...";
            Mouse.SetCursor(System.Windows.Input.Cursors.Wait);
            statusProgressBar.Value = 0;
            DoEvents();

            //シナリオの削除
            scenarioSelectListView.Items.Clear();
            scenarioManager = new ScenarioDataManagement();
            ClearScenarioInfo();

            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath, "*");

                //プログレスバーの準備
                float incVal = (float)100 / files.Length;

                //リストビューの準備
                Wf.ImageList imgList = new Wf.ImageList();
                imgList.ImageSize = ThumbnailSize;
                scenarioSelectListView.LargeImageList = imgList;

                //シナリオの読み込み
                List<Scenario> scenarios = new List<Scenario>();
                List<Wf.ListViewItem> addItems = new List<Wf.ListViewItem>();
                foreach (string file in files)
                {
                    Scenario scenario = new Scenario(file);
                    if (scenario.Load())
                    {
                        scenarios.Add(scenario);
                        addItems.Add(scenario.CreateListViewItem(scenarioSelectListView));
                    }
                    statusProgressBar.Value += incVal;
                    DoEvents();
                }

                scenarioSelectListView.Items.AddRange(addItems.ToArray());
                scenarioManager.SetNewMemento(scenarios);
                GroupingFor(groupIdx);

                statusProgressBar.Value = 100;
                statusText.Text = "読み込み完了";
                Mouse.SetCursor(Cursors.Arrow);
            }
            else
            {
                //ディレクトリが存在しない
                scenarioSelectListView.Clear();

                statusText.Text = "指定されたディレクトリが存在しません。";
                statusProgressBar.Value = 0;
            }
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public MainWindow()
        {
            Wf.Application.EnableVisualStyles();
            InitializeComponent();
            InitializeContextMenu();

            ThumbnailSize = new System.Drawing.Size(96, 96);
            scenarioSelectListView.View = Wf.View.LargeIcon;

            ClearScenarioInfo();

            //Bve標準ディレクトリの取得
            dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Bvets\Scenarios";
        }
    }
}
