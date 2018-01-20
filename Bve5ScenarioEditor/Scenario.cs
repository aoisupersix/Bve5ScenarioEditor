using System;
using System.IO;
using System.Windows.Forms;
using Bve5_Parsing.ScenarioGrammar;
using Hnx8.ReadJEnc;

namespace Bve5ScenarioEditor
{
    class Scenario
    {
        public FileInfo File { get; private set; }
        public ScenarioData Data { get; set; }

        /// <summary>
        /// ファイルパスを指定してシナリオを作成します。
        /// </summary>
        /// <param name="path">シナリオファイルのパス</param>
        public Scenario(string path)
        {
            File = new FileInfo(path);
        }

        /// <summary>
        /// シナリオファイルを読み込んで解析します。
        /// </summary>
        /// <returns>シナリオが正常に読み込めたかどうか</returns>
        public bool Load()
        {
            //ReadJEncを利用してファイルの読み込み
            using (Hnx8.ReadJEnc.FileReader reader = new FileReader(this.File))
            {
                Hnx8.ReadJEnc.CharCode c = reader.Read(this.File);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("Loading ScenarioFile【{0}】...", this.File.Name);
                string text = reader.Text;
                if (c is CharCode.Text)
                {
                    //読み込んだファイルがテキスト
                    Console.WriteLine("Encoding: {0}", c.Name);
                    ScenarioGrammarParser parser = new ScenarioGrammarParser();

                    try
                    {
                        this.Data = parser.Parse(text);
                        Console.WriteLine("Parse Successful !");
                        return true;
                    }
                    catch (NullReferenceException)
                    {
                        Console.Error.WriteLine("Scenario: 【{0}】header mismatched.", this.File.FullName);
                    }
                }
                else
                {
                    //テキストファイルではない
                    Console.Error.WriteLine("【{0}】is not text file.", this.File.FullName);
                }
            }
            Console.Error.WriteLine("Scenario Load Error");
            return false;
        }

        /// <summary>
        /// シナリオをアイテムに追加したリストビューを返します。
        /// </summary>
        /// <param name="listView">追加する対象のリストビュー</param>
        /// <returns>シナリオを追加したリストビュー</returns>
        public ListView AddListViewItem(ListView listView)
        {
            ListViewItem item = new ListViewItem(Data.Title);

            listView.Items.Add(item);
            return listView;
        }
    }
}
