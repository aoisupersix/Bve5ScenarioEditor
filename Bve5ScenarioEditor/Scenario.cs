using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Bve5_Parsing.ScenarioGrammar;
using Hnx8.ReadJEnc;
using Antlr4.Runtime;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// シナリオデータクラス
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// シナリオを編集したかどうか
        /// </summary>
        public bool DidEdit { get; set; }

        /// <summary>
        /// シナリオを削除したかどうか
        /// </summary>
        public bool DidDelete { get; set; }

        /// <summary>
        /// シナリオのファイルパス
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// シナリオのファイル名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// シナリオ情報
        /// </summary>
        public ScenarioData Data { get; set; }

        public ScenarioErrorListener ErrorListener { get; set; }

        /// <summary>
        /// リストビューのアイテム
        /// </summary>
        public ListViewItem Item { get; private set; }

        /// <summary>
        /// リストビューに追加する各情報のサブアイテムインデックス
        /// </summary>
        public enum SubItemIndex
        {
            TITLE, ROUTE_TITLE, VEHICLE_TITLE, AUTHOR, FILE_NAME
        }

        /// <summary>
        /// シナリオデータの初期化をします。
        /// </summary>
        public void InitData()
        {
            Data = Data ?? new ScenarioData();
            Data.Version = Data.Version ?? "";
            Data.Encoding = Data.Encoding ?? "";
            Data.Title = Data.Title ?? "";
            Data.RouteTitle = Data.RouteTitle ?? "";
            Data.VehicleTitle = Data.VehicleTitle ?? "";
            Data.Image = Data.Image ?? "";
            Data.Author = Data.Author ?? "";
            Data.Comment = Data.Comment ?? "";
            if (Data.Route == null)
                Data.Route = new List<FilePath>();
            if (Data.Vehicle == null)
                Data.Vehicle = new List<FilePath>();
        }

        /// <summary>
        /// ファイルパスを指定して新しいインスタンスを作成します。
        /// </summary>
        /// <param name="path">シナリオファイルのパス</param>
        public Scenario(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            ErrorListener = new ScenarioErrorListener();
        }

        /// <summary>
        /// 現在のシナリオデータをコピーしたインスタンスを返します。
        /// </summary>
        /// <returns>現在のシナリオデータのコピー</returns>
        public Scenario Copy()
        {
            Scenario copy = new Scenario(FilePath);
            copy.DidEdit = this.DidEdit;
            copy.DidDelete = this.DidDelete;
            copy.Data = new ScenarioData()
            {
                Version = this.Data.Version,
                Encoding = this.Data.Encoding,
                Image = this.Data.Image,
                Title = this.Data.Title,
                Route = new List<FilePath>(this.Data.Route),
                Vehicle = new List<FilePath>(this.Data.Vehicle),
                RouteTitle = this.Data.RouteTitle,
                VehicleTitle = this.Data.VehicleTitle,
                Author = this.Data.Author,
                Comment = this.Data.Comment
            };
            copy.Item = this.Item;

            return copy;
        }

        /// <summary>
        /// シナリオファイルを読み込んで解析します。
        /// </summary>
        /// <returns>シナリオが正常に読み込めたかどうか</returns>
        public bool Load()
        {
            if (!File.Exists(FilePath))
            {
                //ファイルが存在しない
                Console.Error.WriteLine("Scenario: 【{0}】not found.", FileName);
                return false;
            }

            //ReadJEncを利用してファイルの読み込み
            FileInfo file = new FileInfo(FilePath);
            using (Hnx8.ReadJEnc.FileReader reader = new FileReader(file))
            {
                Hnx8.ReadJEnc.CharCode c = reader.Read(file);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("Loading ScenarioFile【{0}】...", FileName);
                string text = reader.Text;
                if (c is CharCode.Text && Regex.IsMatch(text, @"BveTs\s*Scenario", RegexOptions.IgnoreCase))
                {
                    //読み込んだファイルがテキスト
                    Console.WriteLine("Encoding: {0}", c.Name);
                    ScenarioGrammarParser parser = new ScenarioGrammarParser();
                    parser.ErrorListener = ErrorListener;
                    try
                    {
                        this.Data = parser.Parse(text);
                        Console.WriteLine("Parse Successful !");
                        InitData();
                        return true;
                    }
                    catch (NullReferenceException)
                    {
                        Console.Error.WriteLine("Scenario: 【{0}】header mismatched.", FileName);
                    }
                }
                else
                {
                    //テキストファイルではない
                    Console.Error.WriteLine("【{0}】is not text file.", FileName);
                }
            }
            Console.Error.WriteLine("Scenario Load Error");
            return false;
        }

        /// <summary>
        /// シナリオデータを指定されたディレクトリにコピー(バックアップ)します。
        /// </summary>
        /// <param name="dir">コピーするディレクトリ</param>
        public void Backup(string dir)
        {
            if (File.Exists(FilePath))
                File.Copy(FilePath, dir + @"\" + FileName);
            else
                Save(dir);
        }

        /// <summary>
        /// シナリオデータ指定されたディレクトリに上書き保存します。
        /// </summary>
        /// <param name="dir">書き込むディレクトリ</param>
        public void Save(string dir)
        {
            //シナリオは削除されているのでスキップ
            if (DidDelete)
                return;

            //書き込むテキストの用意
            string text = "";
            string version = Data.Version.Equals("") ? "2.00" : Data.Version;

            //ヘッダー
            text += "Bvets Scenario " + version;
            if(!Data.Encoding.Equals("")) { text += ":" + Data.Encoding; }
            text += Environment.NewLine + Environment.NewLine;

            //各シナリオデータ
            //路線ファイル
            if(Data.Route.Count == 1)
                text += "Route = " + Data.Route[0].Value + Environment.NewLine;
            else if(Data.Route.Count > 1)
            {
                text += "Route = ";
                for(int i = 0; i < Data.Route.Count - 1; i++)
                    text += Data.Route[i].Value + " * " + Data.Route[i].Weight + " | ";
                text += Data.Route[Data.Route.Count - 1].Value + " * " + Data.Route[Data.Route.Count - 1].Weight + Environment.NewLine;
            }
            //車両ファイル
            if (Data.Vehicle.Count == 1)
                text += "Vehicle = " + Data.Vehicle[0].Value + Environment.NewLine;
            else if (Data.Vehicle.Count > 1)
            {
                text += "Vehicle = ";
                for (int i = 0; i < Data.Vehicle.Count - 1; i++)
                    text += Data.Vehicle[i].Value + " * " + Data.Vehicle[i].Weight + " | ";
                text += Data.Vehicle[Data.Vehicle.Count - 1].Value + " * " + Data.Vehicle[Data.Vehicle.Count - 1].Weight + Environment.NewLine;
            }
            //タイトル
            if (!Data.Title.Equals(""))
                text += "Title = " + Data.Title + Environment.NewLine;
            //サムネイル画像
            if (!Data.Image.Equals(""))
                text += "Image = " + Data.Image + Environment.NewLine;
            //路線名
            if (!Data.RouteTitle.Equals(""))
                text += "RouteTitle = " + Data.RouteTitle + Environment.NewLine;
            //車両名
            if (!Data.VehicleTitle.Equals(""))
                text += "VehicleTitle = " + Data.VehicleTitle + Environment.NewLine;
            //作者
            if (!Data.Author.Equals(""))
                text += "Author = " + Data.Author + Environment.NewLine;
            //コメント
            if (!Data.Comment.Equals(""))
                text += "Comment = " + Data.Comment + Environment.NewLine;

            //保存
            string savePath = dir + @"\" + FileName;
            StreamWriter sw = new StreamWriter(savePath, false, Encoding.GetEncoding(Data.Encoding.Equals("") ? "utf-8" : Data.Encoding));
            sw.Write(text);
            sw.Close();
        }

        /// <summary>
        /// このシナリオのlistViewItemを作成します。
        /// </summary>
        /// <param name="listView">対象のlistView</param>
        public ListViewItem CreateListViewItem(ListView listView)
        {
            Item = new ListViewItem(Data.Title);
            //シナリオ情報の設定
            Item.Name = Data.Title ?? "";

            //SubItemIndexに合わせてSubItemを代入
            string[] subItems = new string[Enum.GetNames(typeof(SubItemIndex)).Length];
            subItems[(int)SubItemIndex.TITLE] = Data.Title ?? "";
            subItems[(int)SubItemIndex.ROUTE_TITLE] = Data.RouteTitle ?? "";
            subItems[(int)SubItemIndex.VEHICLE_TITLE] = Data.VehicleTitle ?? "";
            subItems[(int)SubItemIndex.AUTHOR] = Data.Author ?? "";
            subItems[(int)SubItemIndex.FILE_NAME] = FileName;

            Item.SubItems.AddRange(subItems);
            Item.SubItems.RemoveAt(0); //なぜかSubItemsが一つ多いのでindex0を削除する。

            //画像の追加
            string dirName = Path.GetDirectoryName(FilePath) + @"\";
            if (Data.Image != null && System.IO.File.Exists(dirName + Data.Image))
            {
                //画像登録
                try
                {
                    if (!listView.LargeImageList.Images.ContainsKey(Data.Image))
                        listView.LargeImageList.Images.Add(Data.Image, ThumbnailModule.CreateThumbnail(dirName + Data.Image, listView.LargeImageList.ImageSize));
                    Image img = Image.FromFile(dirName + Data.Image);
                    Item.ImageIndex = listView.LargeImageList.Images.IndexOfKey(Data.Image);
                }
                catch (Exception) { Console.Error.WriteLine("Scenario: Image not active format."); }

            }

            return Item;
        }

        /// <summary>
        /// このシナリオをグループに追加します。
        /// </summary>
        /// <param name="listView">対象とするlistView</param>
        /// <param name="item"></param>
        /// <param name="sortItemIdx"></param>
        public void AddGroup(ListView listView, int sortItemIdx)
        {
            if(Item == null || (int)SubItemIndex.FILE_NAME == sortItemIdx)
                return;
            
            //グループの追加
            string groupingText = Item.SubItems[sortItemIdx].Text;
            if (groupingText != "")
            {
                int groupIndex = -1;
                for (int i = 0; i < listView.Groups.Count; i++)
                {
                    if (listView.Groups[i].Header.Equals(groupingText))
                        groupIndex = i;
                }
                if (groupIndex == -1)
                {
                    //グループがないので登録
                    ListViewGroup group = new ListViewGroup(groupingText, HorizontalAlignment.Left);
                    listView.Groups.Add(group);
                    groupIndex = listView.Groups.Count - 1;
                }

                Item.Group = listView.Groups[groupIndex];
            }
        }
    }
}
