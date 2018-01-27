using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Wf = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;

using MahApps.Metro.Controls;
using Bve5_Parsing.ScenarioGrammar;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : MetroWindow
    {
        /// <summary>
        /// 編集するデータのディレクトリパス
        /// </summary>
        string directoryPath;

        /// <summary>
        /// EditWindowのデータソース
        /// </summary>
        EditWindowViewModel dataSource;

        /// <summary>
        /// 行った編集を適用するかを表すフラグ
        /// </summary>
        bool isEditApply = false;

        /// <summary>
        /// 編集前の初期データ
        /// </summary>
        ScenarioData originalData;

        /// <summary>
        /// 初期データを作成します。
        /// </summary>
        /// <param name="scenarios">編集前のデータ</param>
        void InitOriginalData(Scenario[] scenarios)
        {
            originalData = new ScenarioData();
            originalData.Route = scenarios[0].Data.Route;
            originalData.Vehicle = scenarios[0].Data.Vehicle;
            //ベースとなるデータを設定
            originalData.Title = scenarios[0].Data.Title ?? "";
            originalData.RouteTitle = scenarios[0].Data.RouteTitle ?? "";
            originalData.VehicleTitle = scenarios[0].Data.VehicleTitle ?? "";
            originalData.Author = scenarios[0].Data.Author ?? "";
            originalData.Comment = scenarios[0].Data.Comment ?? "";
            originalData.Image = scenarios[0].Data.Image ?? "";

            //ベースと異なる項目は「複数...」に変更
            if (scenarios.Count(x => originalData.Title.Equals(x.Data.Title ?? "")) < scenarios.Length)
                originalData.Title = "複数タイトル...";
            if (scenarios.Count(x => originalData.RouteTitle.Equals(x.Data.RouteTitle ?? "")) < scenarios.Length)
                originalData.RouteTitle = "複数路線名...";
            if (scenarios.Count(x => originalData.VehicleTitle.Equals(x.Data.VehicleTitle ?? "")) < scenarios.Length)
                originalData.VehicleTitle = "複数車両名...";
            if (scenarios.Count(x => originalData.Author.Equals(x.Data.Author ?? "")) < scenarios.Length)
                originalData.Author = "複数作者名...";
            if (scenarios.Count(x => originalData.Comment.Equals(x.Data.Comment ?? "")) < scenarios.Length)
                originalData.Comment = "複数コメント...";
            if (scenarios.Count(x => originalData.Image.Equals(x.Data.Image ?? "")) < scenarios.Length)
                originalData.Image = "";

            //ファイル参照が複数ある場合は空に
            if(scenarios.Length > 1)
            {
                originalData.Route = new List<FilePath>();
                originalData.Vehicle = new List<FilePath>();
            }
        }

        /// <summary>
        /// シナリオ情報データソースに初期値を代入します。
        /// </summary>
        /// <param name="fileName">シナリオファイル名</param>
        void InitTextDataSource(string fileName)
        {
            dataSource = new EditWindowViewModel();
            this.DataContext = dataSource;

            dataSource.DirPath = this.directoryPath;
            dataSource.Title = originalData.Title;
            dataSource.RouteTitle = originalData.RouteTitle;
            dataSource.VehicleTitle = originalData.VehicleTitle;
            dataSource.Author = originalData.Author;
            dataSource.Comment = originalData.Comment;
            dataSource.FileName = GetRelativeFilePath(directoryPath, fileName);
            dataSource.ImagePath = originalData.Image;
        }

        /// <summary>
        /// ファイルの選択確率を更新します。
        /// </summary>
        void UpdateFileReferenceProbability()
        {
            //路線ファイル
            double weightSum = dataSource.RoutePathList.Select(x => double.Parse(x.Weight)).Sum();
            foreach(var item in dataSource.RoutePathList)
            {
                item.Probability = Math.Round(double.Parse(item.Weight) / weightSum, 2) * 100 + "%";
            }
            //車両ファイル
            weightSum = dataSource.VehiclePathList.Select(x => double.Parse(x.Weight)).Sum();
            foreach(var item in dataSource.VehiclePathList)
            {
                item.Probability = Math.Round(double.Parse(item.Weight) / weightSum, 2) * 100 + "%";
            }
        }

        /// <summary>
        /// ファイル参照情報をUIに表示します。
        /// </summary>
        /// <param name="scenarios">表示するシナリオデータ</param>
        void ShowFileReferenceInfo(Scenario[] scenarios)
        {
            Scenario scenario = scenarios[0];
            //路線ファイル
            int count = scenarios.Count(x => x.Data.Route.SequenceEqual(scenario.Data.Route));
            if (count == scenarios.Length)
            {
                foreach (var filePath in scenario.Data.Route)
                {
                    dataSource.RoutePathList.Add(new FileRef_ListViewItem
                    {
                        FilePath = filePath.Value,
                        Weight = filePath.Weight.ToString(),
                        Probability = "1"
                    });
                }
            }

            //車両ファイル
            count = scenarios.Count(x => x.Data.Vehicle.SequenceEqual(scenario.Data.Vehicle));
            if (count == scenarios.Length)
            {
                foreach (var filePath in scenario.Data.Vehicle)
                {
                    dataSource.VehiclePathList.Add(new FileRef_ListViewItem
                    {
                        FilePath = filePath.Value,
                        Weight = filePath.Weight.ToString(),
                        Probability = "1"
                    });
                }
            }

            //選択確率更新
            UpdateFileReferenceProbability();
        }

        /// <summary>
        /// シナリオデータに行った編集を適用します。
        /// </summary>
        /// <param name="scenarios">編集を適用するシナリオデータ</param>
        void SetEditData(Scenario[] scenarios)
        {
            //ファイル参照の差分確認用ダミーを作成する
            var dummyScenario = new ScenarioData();
            dummyScenario.Route = new List<FilePath>();
            dummyScenario.Vehicle = new List<FilePath>();

            //路線ファイル
            foreach (var item in dataSource.RoutePathList)
            {
                dummyScenario.Route.Add(new FilePath
                {
                    Value = item.FilePath,
                    Weight = double.Parse(item.Weight)
                });
            }
            //車両ファイル
            foreach (var item in dataSource.VehiclePathList)
            {
                dummyScenario.Vehicle.Add(new FilePath
                {
                    Value = item.FilePath,
                    Weight = double.Parse(item.Weight)
                });
            }

            //編集されたデータを更新
            foreach (var scenario in scenarios)
            {
                //タイトル
                if (!dataSource.Title.Equals(originalData.Title))
                {
                    scenario.DidEdit = true;
                    scenario.Data.Title = dataSource.Title;
                }
                //路線名
                if (!dataSource.RouteTitle.Equals(originalData.RouteTitle))
                {
                    scenario.DidEdit = true;
                    scenario.Data.RouteTitle = dataSource.RouteTitle;
                }
                //車両名
                if (!dataSource.VehicleTitle.Equals(originalData.VehicleTitle))
                {
                    scenario.DidEdit = true;
                    scenario.Data.VehicleTitle = dataSource.VehicleTitle;
                }
                //作者名
                if (!dataSource.Author.Equals(originalData.Author))
                {
                    scenario.DidEdit = true;
                    scenario.Data.Author = dataSource.Author;
                }
                //コメント
                if (!dataSource.Comment.Equals(originalData.Comment))
                {
                    scenario.DidEdit = true;
                    scenario.Data.Comment = dataSource.Comment;
                }
                //画像パス
                if (!dataSource.ImagePath.Equals(originalData.Image))
                {
                    scenario.DidEdit = true;
                    scenario.Data.Image = dataSource.ImagePath;
                }

                //路線ファイル
                if (!dummyScenario.Route.SequenceEqual(originalData.Route))
                {
                    //編集
                    scenario.DidEdit = true;
                    scenario.Data.Route = dummyScenario.Route;
                }

                //車両ファイル
                if (!dummyScenario.Vehicle.SequenceEqual(originalData.Vehicle))
                {
                    //編集
                    scenario.DidEdit = true;
                    scenario.Data.Vehicle = dummyScenario.Vehicle;
                }
            }
        }

        /// <summary>
        /// 絶対パスから相対パスを取得します。
        /// </summary>
        /// <param name="nowDirectoryPath">現在のディレクトリパス</param>
        /// <param name="absolutePath">目的ファイルの絶対パス</param>
        /// <returns></returns>
        string GetRelativeFilePath(string nowDirectoryPath, string absolutePath)
        {
            Uri nowUri = new Uri(nowDirectoryPath);
            Uri absoluteUri = new Uri(absolutePath);
            Uri relativeUri = nowUri.MakeRelativeUri(absoluteUri);

            return relativeUri.ToString().Replace("/", @"\");
        }

        #region EventHandler

        /// <summary>
        /// ファイル参照の選択確率を更新します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateFileReferenceProbability();
        }

        /// <summary>
        /// 重み付け係数のテキストボックス入力受付を数字のみに制限します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void WeightTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex(@"[0-9.]").IsMatch(e.Text);
        }

        /// <summary>
        /// 重み付け係数のテキストボックスの入力をシナリオデータに反映します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void WeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            //TextBoxの最後が'.'の場合は無視する
            if (textBox.Text.Length > 0 && textBox.Text[textBox.Text.Length - 1] == '.')
                return;

            if (double.TryParse(textBox.Text, out double weight))
            {
                //入力値が正しい場合は更新
                if (textBox.Name.Equals("routeWeightTextBox"))
                {
                    //路線ファイルの重み付け係数更新
                    var selectedItem = routeListView.SelectedItem as FileRef_ListViewItem;
                    selectedItem.Weight = weight.ToString();
                }
                else
                {
                    //路線ファイルの重み付け係数更新
                    var selectedItem = vehicleListView.SelectedItem as FileRef_ListViewItem;
                    selectedItem.Weight = weight.ToString();
                }
            }
        }

        /// <summary>
        /// ファイル参照を追加します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void FileReferenceAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(routeAddButton))
            {
                //路線ファイル参照を追加
                dataSource.RoutePathList.Add(new FileRef_ListViewItem
                {
                    FilePath = "new route",
                    Weight = "1",
                    Probability = "1" //確率はバックパッチで当てる
                });
                UpdateFileReferenceProbability();
            }
            else
            {
                //車両ファイル参照を追加
                dataSource.VehiclePathList.Add(new FileRef_ListViewItem
                {
                    FilePath = "new vehicle",
                    Weight = "1",
                    Probability = "1" //確率はバックパッチで当てる
                });
                UpdateFileReferenceProbability();
            }
        }

        /// <summary>
        /// 選択されているファイル参照を削除します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void FileReferenceDeleteButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender.Equals(routeDeleteButton))
            {
                if(routeListView.SelectedIndex == -1)
                    //アイテムが選択されていない
                    return;

                //対応する路線ファイル参照を削除
                var deleteItem = routeListView.SelectedItem as FileRef_ListViewItem;
                dataSource.RoutePathList.Remove(deleteItem);
                UpdateFileReferenceProbability();
            }
            else
            {
                if (vehicleListView.SelectedIndex == -1)
                    //アイテムが選択されていない
                    return;

                //対応する路線ファイル参照を削除
                var deleteItem = vehicleListView.SelectedItem as FileRef_ListViewItem;
                dataSource.VehiclePathList.Remove(deleteItem);
                UpdateFileReferenceProbability();
            }
        }

        /// <summary>
        /// ファイル参照を更新します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void FileReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wf.OpenFileDialog();
            if (Directory.Exists(directoryPath))
                dlg.InitialDirectory = directoryPath;
            dlg.Title = "ファイルを選択";
            dlg.Filter = "全てのファイル(*.*)|*.*|テキストファイル(*.txt)|*.txt";
            if (dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                //シナリオ情報を更新
                string val = GetRelativeFilePath(directoryPath + @"\", dlg.FileName);
                var button = (Button)sender;
                if (button.Name.Equals("routeReferenceButton"))
                {
                    //路線ファイル更新
                    var item = routeListView.SelectedItem as FileRef_ListViewItem;
                    item.FilePath = val;
                }
                else
                {
                    //車両ファイル更新
                    var item = vehicleListView.SelectedItem as FileRef_ListViewItem;
                    item.FilePath = val;
                }
            }
        }

        /// <summary>
        /// シナリオの画像ファイル参照を更新します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void ImageReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wf.OpenFileDialog();
            if (Directory.Exists(directoryPath))
                dlg.InitialDirectory = directoryPath;
            dlg.Title = "ファイルを選択";
            dlg.Filter = "画像ファイル(*.png,*.jpg,*.bmp,*.gif)|*.png;*.jpg;*.bmp;*.gif|すべてのファイル(*.*)|*.*";
            if (dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                string path = GetRelativeFilePath(directoryPath + @"\", dlg.FileName);
                dataSource.ImagePath = path;
            }
        }

        void RouteListViewColumn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// OKボタンを押した際に編集を適応してウインドウを閉じます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            isEditApply = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタンを押した際にウインドウを閉じます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion EventHandler

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public EditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウインドウをダイアログとして表示します。
        /// </summary>
        /// <param name="editData">編集するシナリオデータ</param>
        /// <returns>編集後のシナリオデータ</returns>
        public Scenario[] ShowWindow(Scenario[] scenarioData)
        {
            directoryPath = scenarioData[0].File.DirectoryName;

            InitOriginalData(scenarioData);
            InitTextDataSource(scenarioData[0].File.FullName);
            ShowFileReferenceInfo(scenarioData);

            //ウインドウタイトルの代入
            dataSource.WindowTitle = "Edit: " + scenarioData[0].Data.Title;
            if (scenarioData.Length > 1)
                dataSource.WindowTitle += " ほか" + scenarioData.Length + "シナリオ";

            //このダイアログの表示
            this.ShowDialog();

            //編集を適用
            if (isEditApply)
                SetEditData(scenarioData);

            return scenarioData;
        }
    }
}
