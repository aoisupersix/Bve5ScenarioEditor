using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Wf = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

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

        EditWindowViewModel scenarioTextDataSource;

        /// <summary>
        /// 路線ファイル参照リストボックスのアイテムリスト
        /// </summary>
        ObservableCollection<FilePathReferenceDataSource> routePathList;

        /// <summary>
        /// 車両ファイル参照リストボックスのアイテムリスト
        /// </summary>
        ObservableCollection<FilePathReferenceDataSource> vehiclePathList;

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
            originalData.Title = scenarios[0].Data.Title;
            originalData.RouteTitle = scenarios[0].Data.RouteTitle;
            originalData.VehicleTitle = scenarios[0].Data.VehicleTitle;
            originalData.Author = scenarios[0].Data.Author;
            originalData.Comment = scenarios[0].Data.Comment;
            originalData.Image = scenarios[0].Data.Image;

            //ベースと異なる項目は「複数...」に変更
            if (scenarios.Count(x => x.Data.Title.Equals(originalData.Title)) < scenarios.Length)
                originalData.Title = "複数タイトル...";
            if (scenarios.Count(x => x.Data.RouteTitle.Equals(originalData.RouteTitle)) < scenarios.Length)
                originalData.RouteTitle = "複数路線名...";
            if (scenarios.Count(x => x.Data.VehicleTitle.Equals(originalData.VehicleTitle)) < scenarios.Length)
                originalData.VehicleTitle = "複数車両名...";
            if (scenarios.Count(x => x.Data.Author.Equals(originalData.Author)) < scenarios.Length)
                originalData.Author = "複数作者名...";
            if (scenarios.Count(x => x.Data.Comment.Equals(originalData.Comment)) < scenarios.Length)
                originalData.Comment = "複数コメント...";
            if (scenarios.Count(x => x.Data.Image.Equals(originalData.Image)) < scenarios.Length)
                originalData.Image = "";
        }

        /// <summary>
        /// シナリオ情報データソースに初期値を代入します。
        /// </summary>
        /// <param name="fileName">シナリオファイル名</param>
        void InitTextDataSource(string fileName)
        {
            scenarioTextDataSource = new EditWindowViewModel();
            this.DataContext = scenarioTextDataSource;

            scenarioTextDataSource.DirPath = this.directoryPath;
            scenarioTextDataSource.Title = originalData.Title;
            scenarioTextDataSource.RouteTitle = originalData.RouteTitle;
            scenarioTextDataSource.VehicleTitle = originalData.VehicleTitle;
            scenarioTextDataSource.Author = originalData.Author;
            scenarioTextDataSource.Comment = originalData.Comment;
            scenarioTextDataSource.FileName = GetRelativeFilePath(directoryPath, fileName);
            scenarioTextDataSource.ImagePath = originalData.Image;
        }

        /// <summary>
        /// ファイルの選択確率を更新します。
        /// </summary>
        void UpdateFileReferenceProbability()
        {
            double weightSum = routePathList.Select(x => double.Parse(x.Weight)).Sum();
            foreach(var item in routePathList)
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
            int count = scenarios.Count(x => x.Data.Route.SequenceEqual(scenario.Data.Route));
            if (count == scenarios.Length)
            {
                double weightSum = scenario.Data.Route.Select(x => x.Weight).Sum();
                foreach (var filePath in scenario.Data.Route.Select((v, i) => new { v, i }))
                {
                    routePathList.Add(new FilePathReferenceDataSource
                    {
                        FilePath = filePath.v.Value,
                        Weight = filePath.v.Weight.ToString(),
                        Probability = Math.Round(filePath.v.Weight / weightSum, 2) * 100 + "%"
                    });
                }
            }
        }

        /// <summary>
        /// シナリオデータに行った編集を適用します。
        /// </summary>
        /// <param name="scenarios">編集を適用するシナリオデータ</param>
        void SetEditData(Scenario[] scenarios)
        {
            //現時点ではファイル参照のみ更新

            //路線ファイル参照
            if(scenarios.Length == 1 || routeListView.Items.Count > 0)
            {
                //ダミーのシナリオを作成し、その差分から編集されたかどうかを確認する
                var dummyScenario = new Bve5_Parsing.ScenarioGrammar.ScenarioData();
                dummyScenario.Route = new List<Bve5_Parsing.ScenarioGrammar.FilePath>();
                dummyScenario.Vehicle = new List<Bve5_Parsing.ScenarioGrammar.FilePath>();
                foreach(var item in routePathList)
                {
                    dummyScenario.Route.Add(new Bve5_Parsing.ScenarioGrammar.FilePath
                    {
                        Value = item.FilePath,
                        Weight = double.Parse(item.Weight)
                    });
                }
                foreach(var scenario in scenarios)
                {
                    if (!scenario.Data.Route.SequenceEqual(dummyScenario.Route))
                    {
                        //編集
                        scenario.DidEdit = true;
                        scenario.Data.Route = dummyScenario.Route;
                    }
                }
            }
        }

        /// <summary>
        /// シナリオ情報の表示を更新します。
        /// </summary>
        void UpdateScenarioInfo(Scenario[] editData, bool isUpdateEditView)
        {
            this.Title = editData.Length > 1 ? "Edit - " + editData[0].Data.Title + " など" + editData.Length + "シナリオ" : "Edit - " + editData[0].Data.Title;
            if(editData.Any(x => x.DidEdit))
                this.Title += "*";
            UpdateFileReferenceProbability();
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
            e.Handled = !new Regex(@"[0-9\.]").IsMatch(e.Text);
        }

        /// <summary>
        /// 重み付け係数のテキストボックスの入力をシナリオデータに反映します。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void WeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (double.TryParse(textBox.Text, out double weight))
            {
                //入力値が正しい場合は更新
                if (textBox.Name.Equals("weightTextBox"))
                {
                    //路線ファイルの重み付け係数更新
                    var selectedItem = (FilePathReferenceDataSource)routeListView.SelectedItem;
                    selectedItem.Weight = weight.ToString();
                }
                else
                {
                    //TODO 車両ファイル
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
                routePathList.Add(new FilePathReferenceDataSource
                {
                    FilePath = "new route",
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
                var deleteItem = (FilePathReferenceDataSource)routeListView.SelectedItem;
                routePathList.Remove(deleteItem);
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
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Bvets\Scenarios";

            var dlg = new Wf.OpenFileDialog();
            if (Directory.Exists(dirPath))
                dlg.InitialDirectory = dirPath;
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
                    var item = (FilePathReferenceDataSource)routeListView.SelectedItem;
                    item.FilePath = val;
                }
                else
                {

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
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Bvets\Scenarios";

            var dlg = new Wf.OpenFileDialog();
            if (Directory.Exists(dirPath))
                dlg.InitialDirectory = dirPath;
            dlg.Title = "ファイルを選択";
            dlg.Filter = "画像ファイル(*.png,*.jpg,*.bmp,*.gif)|*.png;*.jpg;*.bmp;*.gif|すべてのファイル(*.*)|*.*";
            if (dlg.ShowDialog() == Wf.DialogResult.OK)
            {
                string path = GetRelativeFilePath(directoryPath + @"\", dlg.FileName);
                imagePathTextBox.Text = path;
            }
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
            routePathList = new ObservableCollection<FilePathReferenceDataSource>();
            routeListView.DataContext = routePathList;
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
            scenarioTextDataSource.WindowTitle = "Edit: " + scenarioData[0].Data.Title;
            if (scenarioData.Length > 1)
                scenarioTextDataSource.WindowTitle += "ほか" + scenarioData.Length + "シナリオ";

            //このダイアログの表示
            this.ShowDialog();

            //編集を適用
            if (isEditApply)
                SetEditData(scenarioData);

            return scenarioData;
        }
    }
}
