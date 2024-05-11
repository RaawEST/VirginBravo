using Simplified;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace VirginBravo.Test
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }
    }

    public class TestVM : BaseInpc
    {
        private int selectedNum;

        public ObservableCollection<int> Nums { get; } = new(Enumerable.Range(1, 9));

        public int SelectedNum { get => selectedNum; set => Set(ref selectedNum, value); }

        public ObservableCollection<int> MultNums { get; } = new();

        protected override void OnPropertyChanged(string? propertyName, object? oldValue, object? newValue)
        {
            base.OnPropertyChanged(propertyName, oldValue, newValue);

            MultNums.Clear(); // Clear
            for (int i = 1; i < 11; i++)
            {

                MultNums.Add(SelectedNum * i);
            }
        }
    }
}
