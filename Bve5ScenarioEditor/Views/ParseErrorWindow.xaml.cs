using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bve5ScenarioEditor.ViewModels;

namespace Bve5ScenarioEditor.Views
{
    /// <summary>
    /// ParseErrorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ParseErrorWindow : Window
    {
        public ParseErrorWindow(List<Scenario> scenarios)
        {
            InitializeComponent();
            ParseErrorWindowViewModel vm = new ParseErrorWindowViewModel();
            vm.TreeViewItems = new List<ParseErrorTreeViewItem>();
            foreach (var scenario in scenarios)
            {
                if (scenario.ErrorListener.Error.Count > 0)
                    vm.TreeViewItems.Add(new ParseErrorTreeViewItem(scenario.FileName, scenario.ErrorListener.Error));
            }
            DataContext = vm;
        }

        public void ShowWindow()
        {

        }
    }
}
