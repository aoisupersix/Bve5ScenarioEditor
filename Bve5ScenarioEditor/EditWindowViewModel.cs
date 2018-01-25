using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
