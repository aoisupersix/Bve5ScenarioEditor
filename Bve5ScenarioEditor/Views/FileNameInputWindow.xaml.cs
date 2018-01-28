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
        public FileNameInputWindow()
        {
            InitializeComponent();
        }

        public void ShowWindow(string dirPath)
        {
            var vm = new InputWindowViewModel(dirPath);
            DataContext = vm;

            this.ShowDialog();
        }
    }
}
