using System;
using System.Drawing;
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
        /// シナリオのサムネイルを縦横比を固定して返します。
        /// </summary>
        /// <param name="path">サムネイル画像のファイルパス</param>
        /// <param name="width">サムネイルの横幅</param>
        /// <param name="height">サムネイルの縦幅</param>
        /// <returns>引数に指定した大きさのサムネイル画像</returns>
        private Image CreateThumbnail(string path, Size imgSize)
        {
            Bitmap originalBitmap = new Bitmap(path);
            //縦横比の計算
            int x, y;
            double w = (double)imgSize.Width / originalBitmap.Width;
            double h = (double)imgSize.Height / originalBitmap.Height;
            if (w <= h)
            {
                x = imgSize.Width;
                y = (int)(imgSize.Width * (w / h));
            }
            else
            {
                x = (int)(imgSize.Height * (h / w));
                y = imgSize.Height;
            }

            //描画位置を計算
            int sx = (imgSize.Width - x) / 2;
            int sy = (imgSize.Height - y) / 2;

            //imagelistに合わせたサムネイルを描画
            Bitmap bitmap = new Bitmap(imgSize.Width, imgSize.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(originalBitmap, sx, sy, x, y);

            return bitmap;
        }

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
            //シナリオ情報の設定
            item.Name = Data.Title;
            item.Text = Data.Title;
            item.SubItems.Add(Data.RouteTitle);
            item.SubItems.Add(Data.VehicleTitle);
            item.SubItems.Add(Data.Author);
            item.SubItems.Add(this.File.Name);


            //画像の追加
            string dirName = this.File.DirectoryName + @"\";
            if (Data.Image != null && System.IO.File.Exists(dirName + Data.Image))
            {
                //画像登録
                try
                {
                    if (!listView.LargeImageList.Images.ContainsKey(Data.Image))
                        listView.LargeImageList.Images.Add(Data.Image, CreateThumbnail(dirName + Data.Image, listView.LargeImageList.ImageSize));
                    Image img = Image.FromFile(dirName + Data.Image);
                    item.ImageIndex = listView.LargeImageList.Images.IndexOfKey(Data.Image);
                }
                catch (Exception) { Console.Error.WriteLine("Scenario: Image not active format."); }

            }

            //グループの追加
            if (Data.RouteTitle != null)
            {
                int groupIndex = -1;
                for (int i = 0; i < listView.Groups.Count; i++)
                {
                    if (listView.Groups[i].Header.Equals(Data.RouteTitle))
                        groupIndex = i;
                }
                if (groupIndex == -1)
                {
                    //グループがないので登録
                    ListViewGroup group = new ListViewGroup(Data.RouteTitle, HorizontalAlignment.Left);
                    listView.Groups.Add(group);
                    groupIndex = listView.Groups.Count - 1;
                }

                item.Group = listView.Groups[groupIndex];
            }

            //リストビューに登録
            listView.Items.Add(item);

            return listView;
        }
    }
}
