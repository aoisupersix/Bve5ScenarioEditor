using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bve5ScenarioEditor.ViewModels
{
    /// <summary>
    /// SettingWindowのビューモデル
    /// </summary>
    class SettingWindowViewModel: ViewModelBase
    {
        bool _isBackupEnabled;
        /// <summary>
        /// バックアップを有効化チェックボックスの状態
        /// </summary>
        public bool IsBackupEnabled
        {
            get { return _isBackupEnabled; }
            set
            {
                _isBackupEnabled = value;
                OnPropertyChanged();
            }
        }

        bool _isAutoLoadEnabled;
        /// <summary>
        /// 自動読み込み有効化チェックボックスの状態
        /// </summary>
        public bool IsAutoLoadEnabled
        {
            get { return _isAutoLoadEnabled; }
            set
            {
                _isAutoLoadEnabled = value;
                OnPropertyChanged();
            }
        }

        string _initialScenarioDirectory;
        /// <summary>
        /// 初期ディレクトリ
        /// </summary>
        public string InitialScenarioDirectory
        {
            get { return _initialScenarioDirectory; }
            set
            {
                _initialScenarioDirectory = value;
                OnPropertyChanged();
            }
        }
    }
}
