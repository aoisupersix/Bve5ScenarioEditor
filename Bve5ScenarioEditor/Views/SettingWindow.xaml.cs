using MahApps.Metro.Controls;
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
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        /// <param name="comboBox">ディレクトリパスを表示するコンボボックス</param>
        public SettingWindow(ComboBox comboBox)
        {
            InitializeComponent();
        }
    }
}
