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

namespace VirginBravo
{
    /// <summary>
    /// Interaction logic for KGBPassword.xaml
    /// </summary>
    public partial class KGBPassword : Window
    {
        ReceiptsWindow receiptsWindow;
        MainWindow mainWindow;
        public KGBPassword(ReceiptsWindow receiptsWindow, MainWindow mainWindow)
        {
            InitializeComponent();
            this.receiptsWindow = receiptsWindow;
            this.mainWindow = mainWindow;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox1.Password == "1")
            {
                MessageBox.Show("Доступ разрешен");
                KGB kgbWindow = new KGB(receiptsWindow, mainWindow);
                kgbWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Доступ запрещен");
            }
        }
    }
}
