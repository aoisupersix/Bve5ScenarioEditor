using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bve5ScenarioEditor.ViewModels
{
    /// <summary>
    /// ParseErrorWindowのビューモデル
    /// </summary>
    class ParseErrorWindowViewModel
    {
    }

    /// <summary>
    /// ツリービューのアイテムのモデル
    /// </summary>
    class ParseErrorTreeViewItem
    {
        private List<ScenarioError> _errors;

        /// <summary>
        /// シナリオファイルのファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// エラー内容
        /// </summary>
        public List<string> ErrorContents
        {
            get
            {
                List<string> contents = new List<string>();
                foreach(var err in _errors)
                {
                    contents.Add(err.Line + "行" + err.Column + "文字目: " + err.Message);
                }
            }
        }

        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        /// <param name="fileName">シナリオファイル名</param>
        /// <param name="err">パーサエラー</param>
        public ParseErrorTreeViewItem(string fileName, List<ScenarioError> err)
        {
            FileName = fileName;
            _errors = err;
        }
    }
}
