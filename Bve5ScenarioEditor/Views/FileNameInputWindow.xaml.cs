using MahApps.Metro.Controls;
using Bve5ScenarioEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bve5ScenarioEditor.Views
{
    /// <summary>
    /// FileNameInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FileNameInputWindow : MetroWindow
    {
        /// <summary>
        /// ファイル名が有効かどうか
        /// </summary>
        bool isEnableFileName = false;

        #region EventHandler
        /// <summary>
        /// OKボタンをクリックした際にウインドウを閉じます。
        /// </summary>
        /// <param name="sender">イベントのソース</param>
        /// <param name="e">イベントのデータ</param>
        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            isEnableFileName = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタンをクリックした際にウインドウを閉じます。
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
        public FileNameInputWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウインドウを表示し、ファイル名が有効な場合はファイル名を返します。
        /// </summary>
        /// <param name="dirPath">ファイルのディレクトリパス</param>
        /// <returns>ファイル名があればファイル名、なければnull</returns>
        public string ShowWindow(string dirPath)
        {
            var vm = new InputWindowViewModel(dirPath);
            DataContext = vm;

            this.ShowDialog();

            if (isEnableFileName)
                return vm.FileName;
            else
                return null;
        }
    }
}
