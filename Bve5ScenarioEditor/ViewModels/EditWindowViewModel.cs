using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// EditWindowのビューモデル
    /// </summary>
    class EditWindowViewModel : ViewModelBase
    {

        ObservableCollection<FileRef_ListViewItem> _routePathList;
        /// <summary>
        /// 路線ファイル参照リストボックスのアイテムリスト
        /// </summary>
        public ObservableCollection<FileRef_ListViewItem> RoutePathList
        {
            get { return _routePathList; }
            set { _routePathList = value; OnPropertyChanged(); }
        }

        ObservableCollection<FileRef_ListViewItem> _vehiclePathList;
        /// <summary>
        /// 車両ファイル参照リストボックスのアイテムリスト
        /// </summary>
        public ObservableCollection<FileRef_ListViewItem> VehiclePathList
        {
            get { return _vehiclePathList; }
            set { _vehiclePathList = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// シナリオファイルのディレクトリパス
        /// </summary>
        public string DirPath { get; set; }

        string _windowTitle;
        /// <summary>
        /// EditWindowのタイトル
        /// </summary>
        public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        string _title;
        /// <summary>
        /// シナリオタイトル
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }

        string _routeTitle;
        /// <summary>
        /// シナリオ路線名
        /// </summary>
        public string RouteTitle
        {
            get { return _routeTitle; }
            set { _routeTitle = value; OnPropertyChanged(); }
        }

        string _vehicleTitle;
        /// <summary>
        /// シナリオ車両名
        /// </summary>
        public string VehicleTitle
        {
            get { return _vehicleTitle; }
            set { _vehicleTitle = value; OnPropertyChanged(); }
        }

        string _author;
        /// <summary>
        /// シナリオ作者名
        /// </summary>
        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged(); }
        }

        string _comment;
        /// <summary>
        /// シナリオコメント
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; OnPropertyChanged(); }
        }

        string _fileName;
        /// <summary>
        /// シナリオファイル名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged(); }
        }

        string _imagePath;
        /// <summary>
        /// シナリオイメージパス
        /// </summary>
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged();
                //Imageを作成
                string fullPath = DirPath + @"\" + value;
                if (System.IO.File.Exists(fullPath))
                    Image = ThumbnailModule.CreateThumbnailImageSource(fullPath, new Size(128, 128));
            }
        }

        ImageSource _image;
        /// <summary>
        /// シナリオサムネイルイメージ
        /// </summary>
        public ImageSource Image
        {
            get { return _image; }
            set { _image = value; OnPropertyChanged(); }
        }

        public EditWindowViewModel()
        {
            _routePathList = new ObservableCollection<FileRef_ListViewItem>();
            _vehiclePathList = new ObservableCollection<FileRef_ListViewItem>();
        }
    }

    /// <summary>
    /// ファイル参照のリストビューアイテム
    /// </summary>
    class FileRef_ListViewItem : ViewModelBase, IDataErrorInfo
    {
        string filePath;
        string weight;
        string probability;

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged(); }
        }
        public string Weight
        {
            get { return weight; }
            set
            {
                weight = value;
                this.OnPropertyChanged();
            }
        }
        public string Probability
        {
            get { return probability; }
            set
            {
                probability = value;
                this.OnPropertyChanged();
            }
        }

        public string Error { get { return null; } }

        //エラーメッセージ
        public string this[string propertyName]
        {
            get
            {
                string result = null;
                switch (propertyName)
                {
                    case "Weight":
                        if (this.Weight == null) return null;

                        double ii;
                        try
                        {
                            ii = double.Parse(this.Weight);
                        }
                        catch (Exception)
                        {
                            result = "重みは係数は整数もしくは少数で入力してください。";
                            break;
                        }
                        break;
                }
                return result;
            }
        }
    }
}
