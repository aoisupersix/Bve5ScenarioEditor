using System.Drawing;
using System.Windows.Media;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// EditWindowのビューモデル
    /// </summary>
    class EditWindowViewModel: ViewModelBase
    {
        //以下各private変数
        string windowTitle;
        string title;
        string routeTitle;
        string vehicleTitle;
        string author;
        string comment;
        string fileName;
        string imagePath;
        ImageSource image;

        /// <summary>
        /// シナリオファイルのディレクトリパス
        /// </summary>
        public string DirPath { get; set; }

        /// <summary>
        /// EditWindowのタイトル
        /// </summary>
        public string WindowTitle
        {
            get { return windowTitle; }
            set { windowTitle = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオタイトル
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオ路線名
        /// </summary>
        public string RouteTitle
        {
            get { return routeTitle; }
            set { routeTitle = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオ車両名
        /// </summary>
        public string VehicleTitle
        {
            get { return vehicleTitle; }
            set { vehicleTitle = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオ作者名
        /// </summary>
        public string Author
        {
            get { return author; }
            set { author = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオコメント
        /// </summary>
        public string Comment
        {
            get { return comment; }
            set { comment = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオファイル名
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオイメージパス
        /// </summary>
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                imagePath = value;
                OnPropertyChanged();
                //Imageを作成
                string fullPath = DirPath + @"\" + value;
                if (System.IO.File.Exists(fullPath))
                    Image = ThumbnailModule.CreateThumbnailImageSource(fullPath, new Size(128, 128));
            }
        }

        /// <summary>
        /// シナリオサムネイルイメージ
        /// </summary>
        public ImageSource Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(); }
        }
    }
}
