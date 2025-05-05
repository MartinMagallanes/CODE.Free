using CODE.Free.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CODE.Free.Views
{
    /// <summary>
    /// Interaction logic for PcfImportTableView.xaml
    /// </summary>
    public partial class PcfImportTableView : Window
    {
        public PcfImportTableView(PcfImportTableViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var text = comboBox.Text;
                var viewModel = DataContext as PcfImportTableViewModel;
                if (viewModel != null)
                {
                    var filteredItems = viewModel.Rows
                        .Where(row => row.ToString().Contains(text, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    comboBox.ItemsSource = filteredItems;
                    comboBox.IsDropDownOpen = true;
                }
            }
        }
    }
}
