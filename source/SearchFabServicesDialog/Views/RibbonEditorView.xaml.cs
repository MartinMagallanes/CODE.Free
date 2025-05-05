using CODE.Free.Models;
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
    /// Interaction logic for ModelessDialogView.xaml
    /// </summary>
    public partial class RibbonEditorView : Window
    {
        public RibbonEditorView(RibbonEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
