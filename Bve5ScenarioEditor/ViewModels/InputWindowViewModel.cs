using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bve5ScenarioEditor.ViewModels
{
    /// <summary>
    /// FileNameInputWindowのViewModel
    /// </summary>
    class InputWindowViewModel: ViewModelBase, INotifyDataErrorInfo
    {
        /// <summary>
        /// 作成するファイルのディレクトリパス
        /// </summary>
        string _dirPath;

        /// <summary>
        /// ファイルパスの入力エラーメッセージ
        /// </summary>
        string _currentError;

        string _fileName;
        /// <summary>
        /// テキストボックスに入力されたファイル名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
                ValidateProperty(value);
            }
        }

        bool _buttonEnable = false;
        /// <summary>
        /// OKボタンを有効化するかどうか
        /// </summary>
        public bool ButtonEnable
        {
            get { return _buttonEnable; }
            set { _buttonEnable = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// テキストボックスに入力されたファイル名を検証します。
        /// </summary>
        /// <param name="value"></param>
        protected void ValidateProperty(string value)
        {
            if (value.Equals("") || value == null)
            {
                _currentError = "ファイル名を入力してください。";
                ButtonEnable = false;
            }
            else if (System.IO.File.Exists(_dirPath + @"\" + value))
            {
                _currentError = "入力されたファイルは既に存在します。別のファイル名を入力してください。";
                ButtonEnable = false;
            }
            else
            {
                _currentError = null;
                ButtonEnable = true;
            }

            OnErrorsChanged();
        }

        /// <summary>
        /// エラーイベントを発生させます。
        /// </summary>
        void OnErrorsChanged()
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("FileName"));
        }

        #region INotifyDataErrorInfoの実装
        /// <summary>
        /// エラーイベントハンドラ
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// エラーを取得します。
        /// </summary>
        /// <param name="propertyName">エラーを取得するプロパティ名</param>
        /// <returns>エラーがあればエラーメッセージのリスト、なければnull</returns>
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (_currentError == null)
                return null;
            else
            {
                var list = new List<string>{ _currentError };
                return list;
            }
        }

        /// <summary>
        /// 現在エラーが発生しているかどうか
        /// </summary>
        public bool HasErrors
        {
            get { return _currentError != null; }
        }
        #endregion INotifyDataErrorInfoの実装

        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        /// <param name="dirPath">現在のディレクトリパス</param>
        public InputWindowViewModel(string dirPath)
        {
            this._dirPath = dirPath;
        }
    }
}
