using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;
using Bve5ScenarioEditor.ViewModels;

namespace Bve5ScenarioEditor.Views
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {

        /// <summary>
        /// ウインドウのデータソース
        /// </summary>
        SettingWindowViewModel dataSource;

        /// <summary>
        /// シナリオディレクトリのコンボボックス
        /// </summary>
        ComboBox comboBox;

        #region EventHandler

        /// <summary>
        /// シナリオディレクトリの履歴を削除します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteComboBoxItems(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PathList = null;

            //コンボボックスの選択されているアイテム以外を削除
            var selectedItem = comboBox.SelectedItem;
            comboBox.Items.Clear();
            if(selectedItem != null)
            {
                comboBox.Items.Add(selectedItem);
                comboBox.SelectedItem = selectedItem;
            }
        }

        /// <summary>
        /// OKボタンを押された際に変更を適用してウインドウを閉じます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //設定に適用
            Properties.Settings.Default.IsEnabledAutoLoad = dataSource.IsAutoLoadEnabled;
            Properties.Settings.Default.ScenarioPath = dataSource.InitialScenarioDirectory;
            Properties.Settings.Default.Save();

            this.Close();
        }

        /// <summary>
        /// キャンセルボタンが押された際にウインドウを閉じます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion EventHandler

        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        /// <param name="comboBox">ディレクトリパスを表示するコンボボックス</param>
        public SettingWindow(ComboBox comboBox)
        {
            InitializeComponent();
            this.comboBox = comboBox;

            //データソースの初期化
            dataSource = new SettingWindowViewModel();
            this.DataContext = dataSource;
            dataSource.IsBackupEnabled = true; //TODO
            dataSource.IsAutoLoadEnabled = Properties.Settings.Default.IsEnabledAutoLoad;
            dataSource.InitialScenarioDirectory = Properties.Settings.Default.ScenarioPath;
        }
    }
}
