using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using Wf = System.Windows.Forms;
using Wpf = System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using System.Threading.Tasks;
using Bve5ScenarioEditor.Views;

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
        Wf.MenuItem contextMenuItem_New = new Wf.MenuItem("新規シナリオを作成(&N)");
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
            var frame = new DispatcherFrame();
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
            contextMenuItem_New.Click += NewScenarioManuItem_Click;
            contextMenuItem_Edit.Click += EditSelectedScenario;
            contextMenuItem_Delete.Click += DeleteSelectedScenario;

            contextMenu.MenuItems.Add(contextMenuItem_New);
            contextMenu.MenuItems.Add(contextMenuItem_Edit);
            contextMenu.MenuItems.Add(contextMenuItem_Delete);
        }

        /// <summary>
        /// リストビューで選択されているシナリオインスタンスのリストを返します。
        /// </summary>
        /// <returns>シナリオインスタンスのリスト</returns>
        List<Scenario> GetSelectedScenario()
        {
            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            var selected = new List<Scenario>();
            foreach (var item in scenarioSelectListView.SelectedItems)
            {
                selected.Add(scenarios.Find(a => a.Item.Equals(item)));
            }

            return selected;
        }

        /// <summary>
        /// シナリオ情報の編集ウインドウを表示します。
        /// </summary>
        void ShowEditWindow(List<Scenario> editScenario)
        {
            if (editScenario.Count > 0)
            {
                List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
                EditWindow editWindow = new EditWindow();
                editWindow.Owner = this;
                Scenario[] returnData = editWindow.ShowWindow(editScenario.ToArray());

                //編集されたシナリオを適用
                foreach (var ret in returnData)
                {
                    ret.CreateListViewItem(scenarioSelectListView);
                    int idx = scenarios.FindIndex(x => x.FilePath.Equals(ret.FilePath));
                    scenarios[idx] = ret;
                }
                scenarioManager.SetNewMemento(scenarios);
                GroupingFor(groupIdx);
            }
        }

        /// <summary>
        /// 選択されているシナリオに削除フラグをつけます。
        /// </summary>
        void DeleteScenario()
        {
            List<Scenario> delData = GetSelectedScenario();
            if (delData.Count > 0)
            {
                List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();

                //削除の確認
                Wf.DialogResult res = Wf.MessageBox.Show(
                    delData.Count + "個のシナリオファイルを削除します。よろしいですか？",
                    "確認",
                    Wf.MessageBoxButtons.OKCancel);

                if (res == Wf.DialogResult.OK)
                {
                    //削除
                    foreach(var del in delData)
                    {
                        var scenario = scenarios.Find(x => x.FilePath == del.FilePath);
                        scenario.DidDelete = true;
                    }
                    scenarioManager.SetNewMemento(scenarios);
                    GroupingFor(groupIdx);
                }
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
                if (!scenario.DidDelete)
                {
                    scenarioSelectListView.Items.Add(scenario.Item);
                    scenario.AddGroup(scenarioSelectListView, (int)subIdx);
                }
            }

            //ウインドウのタイトルを変更
            int editNum = scenarioManager.NewestSnapEditCount();
            if (editNum > 0)
                this.Title = "Bve5 Scenario Editor [" + editNum + "個のシナリオを編集中]";
            else
                this.Title = "Bve5 Scenario Editor";
        }

        /// <summary>
        /// 表示切り替えメニューのアイテムを排他チェックします。
        /// </summary>
        /// <param name="displayMenuItem">チェックするメニューアイテム</param>
        void CheckDisplayMenuItem(object displayMenuItem)
        {
            var checkItem = displayMenuItem as Wpf.MenuItem;
            checkItem.IsChecked = true;

            //checkItem以外のMenuItemのチェックを外す
            foreach (Wpf.MenuItem item in menuItem_Display.Items)
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
            var checkItem = sortMenuItem as Wpf.MenuItem;
            checkItem.IsChecked = true;

            //checkItem以外のMenuItemのチェックを外す
            foreach (Wpf.MenuItem item in menuItem_Sort.Items)
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
            scenarioVehicleTitleText.Visibility = Visibility.Collapsed;
            scenarioAuthorText.Visibility = Visibility.Collapsed;
            scenarioFileNameText.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 新規シナリオを作成します。
        /// </summary>
        void CreateNewScenario()
        {
            FileNameInputWindow inputWindow = new FileNameInputWindow();
            inputWindow.Owner = this;
            string fileName = inputWindow.ShowWindow(dirPath);
            if(fileName != null)
            {
                List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
                var newScenario = new Scenario(dirPath + @"\" + fileName);
                newScenario.Data = new Bve5_Parsing.ScenarioGrammar.ScenarioData();
                newScenario.Data.Title = "new scenario";
                newScenario.InitData();
                scenarios.Add(newScenario);
                scenarioManager.SetNewMemento(scenarios);
                ShowEditWindow(new List<Scenario>() { newScenario });
            }
        }

        /// <summary>
        /// 現在のディレクトリにあるシナリオファイルを読み込みます。
        /// </summary>
        async void LoadScenarios()
        {
            //バックアップ完了まで保存とパスの変更は不可にする
            menuItem_OverwriteSave.IsEnabled = false;
            menuItem_OtherDirSave.IsEnabled = false;
            menuItem_ShowParseError.IsEnabled = false;
            filePathComboBox.IsEnabled = false;
            referenceButton.IsEnabled = false;

            statusText.Text = "シナリオの読み込み中...";
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
                var imgList = new Wf.ImageList();
                imgList.ImageSize = ThumbnailSize;
                scenarioSelectListView.LargeImageList = imgList;

                //シナリオの読み込み
                var scenarios = new List<Scenario>();
                List<Wf.ListViewItem> addItems = new List<Wf.ListViewItem>();
                foreach (string file in files)
                {
                    var scenario = new Scenario(file);
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

                //パースエラー表示の有効化
                menuItem_ShowParseError.IsEnabled = true;
            }
            else
            {
                //ディレクトリが存在しない
                scenarioSelectListView.Clear();

                statusText.Text = "指定されたディレクトリが存在しません。";
                statusProgressBar.Value = 0;
            }
            //読み込んだシナリオをバックアップ
            await BackupScenarioAsync();

            //保存とパス変更の有効化
            menuItem_OverwriteSave.IsEnabled = true;
            menuItem_OtherDirSave.IsEnabled = true;
            filePathComboBox.IsEnabled = true;
            referenceButton.IsEnabled = true;
        }

        /// <summary>
        /// シナリオファイルを破棄してよいか確認します。
        /// </summary>
        /// <returns></returns>
        bool CheckScenarioDiscard()
        {
            if (scenarioManager == null)
                return true;
            int count = scenarioManager.NewestSnapEditCount();
            if (count == 0)
                return true;
            Wf.DialogResult res = Wf.MessageBox.Show(
                count + "個のシナリオファイルの編集が未保存です。保存しますか？",
                "確認",
                Wf.MessageBoxButtons.YesNoCancel);
            if (res == Wf.DialogResult.Yes)
            {
                //上書き保存
                SaveScenario(dirPath, false);
                return true;
            }
            else if (res == Wf.DialogResult.No)
                return true;

            return false;
        }

        /// <summary>
        /// シナリオのバックアップを非同期で行います。
        /// </summary>
        /// <returns>voidにできないのでTaskを返す</returns>
        async Task BackupScenarioAsync()
        {
            statusText.Text = "シナリオのバックアップ中...";
            statusProgressBar.Value = 0;
            DoEvents();

            Progress<float> progress = new Progress<float>(OnProgressChanged);

            //非同期で行う
            Task task = Task.Run(() =>
            {
                BackupScenario(progress);
            });
            //タスク完了を待つ
            await task;

            statusProgressBar.Value = 100;
            statusText.Text = "バックアップ完了";
        }

        /// <summary>
        /// シナリオファイルをバックアップします。
        /// </summary>
        void BackupScenario(IProgress<float> progress)
        {
            //ディレクトリの準備
            string backupDir = Directory.GetCurrentDirectory() + @"\Backup";
            if(!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            else
            {
                //ディレクトリを一旦削除して再び作る
                DirectoryInfo dirInfo = new DirectoryInfo(backupDir);
                dirInfo.Delete(true);
                Directory.CreateDirectory(backupDir);
            }
            List<Scenario> scenarios = scenarioManager.GetOldestSnapShot();

            //プログレスバーの準備
            float incVal = (float)100 / scenarios.Count;
            float val = 0;

            //バックアップ
            foreach (var scenario in scenarios)
            {
                scenario.Backup(backupDir);
                val += incVal;
                progress.Report(val);
            }

        }

        /// <summary>
        /// シナリオファイルを指定されたディレクトリに保存します。
        /// </summary>
        /// <param name="dir">保存するディレクトリパス</param>
        /// <param name="isSaveAllData">trueを指定すると編集していないデータも保存します。</param>
        void SaveScenario(string dir, bool isSaveAllData)
        {
            //ディレクトリの存在チェック
            if (!Directory.Exists(dir))
                return;

            statusText.Text = "シナリオの保存中...";
            statusProgressBar.Value = 0;
            DoEvents();

            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            float incVal = (float)100 / scenarios.Count;
            //保存
            try
            {
                foreach(var scenario in scenarios)
                {
                    if (scenario.DidDelete)
                    {
                        //削除
                        if (File.Exists(dir + @"\" + scenario.FileName))
                            File.Delete(dir + @"\" + scenario.FileName);
                    }
                    else
                    {
                        if (isSaveAllData)
                        {
                            //全てのシナリオを保存
                            if (!scenario.DidDelete)
                                scenario.Save(dir);
                        }
                        else
                        {
                            //編集されたシナリオのみ保存
                            if (scenario.DidEdit)
                                scenario.Save(dir);
                        }
                    }

                    statusProgressBar.Value += incVal;
                    DoEvents();
                }

                //後処理
                foreach(var scenario in scenarios)
                {
                    scenario.DidEdit = false;
                    if(scenario.DidDelete)
                        scenarios.Remove(scenario);
                }
                scenarioManager.SetNewMemento(scenarios);
                if (filePathComboBox.Items.IndexOf(dir) != -1)
                    filePathComboBox.SelectedIndex = filePathComboBox.Items.IndexOf(dir);
                else
                {
                    filePathComboBox.Items.Add(dir);
                    filePathComboBox.SelectedIndex = filePathComboBox.Items.Count - 1;
                }
                statusProgressBar.Value = 100;
                statusText.Text = dir + "に保存完了";
                Mouse.SetCursor(Cursors.Arrow);
            }
            catch(Exception e)
            {
                MessageBox.Show("保存に失敗しました。:" + e.Message, "エラー");
                statusProgressBar.Value = 0;
                statusText.Text = "シナリオの保存に失敗しました。";
            }
        }

        #region EventHandler
        /// <summary>
        /// Windowがレンダリングされた後、コンボボックスにファイルパスを追加します。初回のみの処理。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void Window_ContentRendered(object sender, EventArgs e)
        {
            //初期ディレクトリがない場合、Bve標準ディレクトリを初期ディレクトリに指定
            if (Properties.Settings.Default.ScenarioPath.Equals(""))
                Properties.Settings.Default.ScenarioPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Bvets\Scenarios";

            if (Properties.Settings.Default.IsEnabledAutoLoad && Directory.Exists(Properties.Settings.Default.ScenarioPath))
            {
                //初期ディレクトリをコンボボックスに追加
                filePathComboBox.Items.Add(Properties.Settings.Default.ScenarioPath);
                filePathComboBox.SelectedIndex = filePathComboBox.Items.Count - 1;
            }
            else
            {
                //初期ディレクトリが存在しない
                statusText.Text = "シナリオファイルのディレクトリを指定してください。";
            }
        }

        /// <summary>
        /// リストビューの選択されているアイテムが変更された際に、シナリオ情報を更新します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ScenarioSelectListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            List<Scenario> selected = GetSelectedScenario();
            statusText.Text = "シナリオを" + selected.Count + "個選択中。";

            ClearScenarioInfo();

            if (selected.Count == 0)
                return;
            else
            {
                //ベースとなるアイテムの取得
                string title = selected[0].Data.Title;
                string routeTitle = selected[0].Data.RouteTitle;
                string vehicleTitle = selected[0].Data.VehicleTitle;
                string author = selected[0].Data.Author;
                string comment = selected[0].Data.Comment;
                string image = selected[0].Data.Image;
                string fileName = selected[0].FileName;

                //ベースと異なる項目を「複数...」に変更
                if (selected.Count(x => x.Data.Title.Equals(title)) < selected.Count)
                    title = "複数タイトル...";
                if (selected.Count(x => x.Data.RouteTitle.Equals(routeTitle)) < selected.Count)
                    routeTitle = "複数路線名...";
                if (selected.Count(x => x.Data.VehicleTitle.Equals(vehicleTitle)) < selected.Count)
                    vehicleTitle = "複数車両名...";
                if (selected.Count(x => x.Data.Author.Equals(author)) < selected.Count)
                    author = "複数作者名...";
                if (selected.Count(x => x.Data.Comment.Equals(comment)) < selected.Count)
                    comment = "複数コメント...";
                if (selected.Count(x => x.Data.Image.Equals(image)) < selected.Count)
                    image = "";

                //サムネイル情報の描画
                if (!image.Equals("") && File.Exists(dirPath + @"\" + image))
                {
                    thumbnailImage.Source = ThumbnailModule.CreateThumbnailImageSource(dirPath + @"\" + image, ThumbnailSize);
                    thumbnailImage.Visibility = Visibility.Visible;
                }

                //情報を表示
                if (!title.Equals(""))
                {
                    scenarioTitleText.Text = title;
                    scenarioTitleText.Visibility = Visibility.Visible;
                }
                if (!routeTitle.Equals(""))
                {
                    scenarioRouteTitleText.Text = routeTitle;
                    scenarioRouteTitleText.Text = routeTitle;
                }
                if (!vehicleTitle.Equals(""))
                {
                    scenarioVehicleTitleText.Text = vehicleTitle;
                    scenarioVehicleTitleText.Visibility = Visibility.Visible;
                }
                if (!author.Equals(""))
                {
                    scenarioAuthorText.Text = author;
                    scenarioAuthorText.Visibility = Visibility.Visible;
                }
                if (!comment.Equals(""))
                {
                    scenarioCommentText.Text = comment;
                    scenarioCommentText.Visibility = Visibility.Visible;
                }
                if (selected.Count == 1)
                {
                    scenarioFileNameText.Text = selected[0].FileName;
                    scenarioFileNameText.Visibility = Visibility.Visible;
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
            ShowEditWindow(GetSelectedScenario());
        }

        /// <summary>
        /// EditWindowを表示し、選択しているシナリオを編集します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void EditSelectedScenario(object sender, RoutedEventArgs e)
        {
            ShowEditWindow(GetSelectedScenario());
        }

        /// <summary>
        /// 選択しているシナリオを削除します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void DeleteSelectedScenario(object sender, RoutedEventArgs e)
        {
            DeleteScenario();
        }

        /// <summary>
        /// 選択しているシナリオを削除します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void DeleteSelectedScenario(object sender, EventArgs e)
        {
            DeleteScenario();
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
                contextMenuItem_New.Visible = false;
                contextMenuItem_Edit.Visible = true;
                contextMenuItem_Delete.Visible = true;
            }
            else
            {
                //メニューを無効化
                contextMenuItem_New.Visible = true;
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
            if (dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                int idx = filePathComboBox.Items.IndexOf(dlg.SelectedPath);
                if(idx == -1)
                {
                    filePathComboBox.Items.Add(dlg.SelectedPath);
                    filePathComboBox.SelectedIndex = filePathComboBox.Items.Count - 1;
                }
                else
                {
                    filePathComboBox.SelectedIndex = idx;
                }
            }
        }

        /// <summary>
        /// コンボボックスのアイテムが変更された際、シナリオを読み込みます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void FilePathComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectVal = (string)filePathComboBox.SelectedValue;
            if (selectVal.Equals(dirPath))
                return;
            if (CheckScenarioDiscard())
            {
                //現在のシナリオを破棄して新たなシナリオデータを生成
                dirPath = selectVal;
                LoadScenarios();
            }
            else
            {
                //コンボボックスのアイテムを元に戻す
                int idx = filePathComboBox.Items.IndexOf(dirPath);
                if(idx == -1)
                {
                    filePathComboBox.Items.Add(dirPath);
                    filePathComboBox.SelectedIndex = filePathComboBox.Items.Count - 1;
                }
                else
                    filePathComboBox.SelectedIndex = idx;
            }

        }

        /// <summary>
        /// パーサエラーウインドウを表示します
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ShowParserErrorWindow(object sender, RoutedEventArgs e)
        {
            List<Scenario> scenarios = scenarioManager.GetNewestSnapShot();
            if(scenarios != null)
            {
                ParseErrorWindow errorWindow = new ParseErrorWindow(scenarios);
                errorWindow.ShowDialog();
            }
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
        /// 新しいシナリオを作成します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void NewScenarioManuItem_Click(object sender, RoutedEventArgs e)
        {
            CreateNewScenario();
        }

        /// <summary>
        /// 新しいシナリオを作成します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void NewScenarioManuItem_Click(object sender, EventArgs e)
        {
            CreateNewScenario();
        }

        /// <summary>
        /// 編集されたシナリオを上書き保存します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void OverwriteSaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int count = scenarioManager.NewestSnapEditCount();
            if (count == 0)
                return;
            Wf.DialogResult res = Wf.MessageBox.Show(count + "個のシナリオファイルの編集を上書き保存します。よろしいですか？", "確認", Wf.MessageBoxButtons.OKCancel);
            if(res == Wf.DialogResult.OK)
            {
                //上書き保存
                SaveScenario(dirPath, false);
            }
        }

        /// <summary>
        /// シナリオファイルを別のディレクトリに保存します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void DirectorySaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wf.FolderBrowserDialog();
            dlg.Description = "シナリオを保存するフォルダを選択してください。";
            dlg.SelectedPath = dirPath;
            if (dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                string dir = dlg.SelectedPath;
                SaveScenario(dir, true);
            }
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

        /// <summary>
        /// ウインドウを閉じた際に呼ばれるイベントハンドラです。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void MetroWindow_Closed(object sender, EventArgs e)
        {
            //コンボボックスのファイルパスを記録
            StringCollection paths = new StringCollection();
            foreach(var item in filePathComboBox.Items)
            {
                paths.Add(item.ToString());
            }
            //設定を保存
            Properties.Settings.Default.PathList = paths;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 非同期処理中のプログレスバーを更新します。
        /// </summary>
        /// <param name="val">プログレスバーの値</param>
        void OnProgressChanged(float val)
        {
            statusProgressBar.Value = val;
        }

        #endregion EventHandler

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public MainWindow()
        {
            Wf.Application.EnableVisualStyles();
            InitializeComponent();
            InitializeContextMenu();
            DataContext = this;

            ThumbnailSize = new System.Drawing.Size(96, 96);
            scenarioSelectListView.View = Wf.View.LargeIcon;

            ClearScenarioInfo();

            //コンボボックスのアイテムを復元
            if(Properties.Settings.Default.PathList != null)
            {
                foreach (var path in Properties.Settings.Default.PathList)
                {
                    filePathComboBox.Items.Add(path);
                }
            }
        }
    }
}
