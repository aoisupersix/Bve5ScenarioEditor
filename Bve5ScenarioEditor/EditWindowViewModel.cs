using System.Drawing;
using System.Windows.Media;

namespace Bve5ScenarioEditor
{
    class EditWindowViewModel: ViewModelBase
    {
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

        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        public string RouteTitle
        {
            get { return routeTitle; }
            set { routeTitle = value; OnPropertyChanged(); }
        }

        public string VehicleTitle
        {
            get { return vehicleTitle; }
            set { vehicleTitle = value; OnPropertyChanged(); }
        }

        public string Author
        {
            get { return author; }
            set { author = value; OnPropertyChanged(); }
        }

        public string Comment
        {
            get { return comment; }
            set { comment = value; OnPropertyChanged(); }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged(); }
        }

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

        public ImageSource Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(); }
        }
    }
}
