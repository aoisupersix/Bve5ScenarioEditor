using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        string dirPath = @"F:\Library\Documents\Bvets\Scenarios"; //TODO

        /// <summary>
        /// 現在のディレクトリにあるシナリオファイルを読み込みます。
        /// </summary>
        void LoadScenarios()
        {

            //とりあえずパスの追加 TODO: あとで修正
            filePathComboBox.Items.Add(dirPath);
            this.filePathComboBox.SelectedIndex = this.filePathComboBox.Items.Count - 1;

            //シナリオの読み込み
            string[] files = Directory.GetFiles(dirPath, "*");
            foreach(string file in files)
            {
                Scenario scenario = new Scenario(file);
                if (scenario.Load())
                {
                    scenarioSelectListView = scenario.AddListViewItem(scenarioSelectListView);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadScenarios();
        }
    }
}
