using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace CODE.Free.ViewModels
{
    public class PcfImportTableViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RowData> _rows;
        public ObservableCollection<RowData> Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                OnPropertyChanged();
            }
        }

        public PcfImportTableViewModel()
        {
            Rows = new ObservableCollection<RowData>
                    {
                        new RowData { Column1 = "Sample1", Column2 = "Option1", Column2Items = new ObservableCollection<string> { "Option1", "Option2", "Option3" } },
                        new RowData { Column1 = "Sample2", Column2 = "Option2", Column2Items = new ObservableCollection<string> { "Option1", "Option2", "Option3" } },
                        new RowData { Column1 = "Sample3", Column2 = "Option3", Column2Items = new ObservableCollection<string> { "Option1", "Option2", "Option3" } }
                    };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowData : INotifyPropertyChanged
    {
        private string _column1;
        private string _column2;
        private ObservableCollection<string> _column2Items;

        public string Column1
        {
            get => _column1;
            set
            {
                _column1 = value;
                OnPropertyChanged();
            }
        }

        public string Column2
        {
            get => _column2;
            set
            {
                _column2 = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Column2Items
        {
            get => _column2Items;
            set
            {
                _column2Items = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
