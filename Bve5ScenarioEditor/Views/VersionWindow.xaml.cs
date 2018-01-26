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

using MahApps.Metro.Controls;


namespace Bve5ScenarioEditor
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : MetroWindow
    {
        public VersionWindow()
        {
            InitializeComponent();
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            versionText.Text = asm.GetName().Version.ToString();
        }
    }
}
