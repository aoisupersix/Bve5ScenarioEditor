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
        public List<ParseErrorTreeViewItem> TreeViewItems { get; set; }
    }

    /// <summary>
    /// ツリービューのアイテムのモデル
    /// </summary>
    class ParseErrorTreeViewItem
    {
        /// <summary>
        /// シナリオファイルのファイル名
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// エラー内容
        /// </summary>
        public List<ParseErrorTreeViewItem> Child { get; set; }

        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        private ParseErrorTreeViewItem() { }

        /// <summary>
        /// 新しいインスタンスを作成します。
        /// </summary>
        /// <param name="fileName">シナリオファイル名</param>
        /// <param name="err">パーサエラー</param>
        public ParseErrorTreeViewItem(string fileName, List<ScenarioError> errors)
        {
            Content = fileName;
            Child = new List<ParseErrorTreeViewItem>();
            foreach (var err in errors)
            {
                Child.Add(
                    new ParseErrorTreeViewItem()
                    {
                        Content = err.Line + "行" + err.Column + "文字目: " + err.Message,
                        Child = null
                    });
            }
        }
    }
}
