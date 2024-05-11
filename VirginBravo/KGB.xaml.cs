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
using System.IO;
using Newtonsoft.Json;

namespace VirginBravo
{
    /// <summary>
    /// Interaction logic for KGB.xaml
    /// </summary>
    public partial class KGB : Window
    {
        ReceiptsWindow receiptsWindow;
        
        MainWindow mainWindow;

        public KGB(ReceiptsWindow receiptsWindow, MainWindow mainWindow)
        {
            InitializeComponent();
            this.receiptsWindow = receiptsWindow;
            ItemPrice.PreviewTextInput += receiptsWindow.NumericTextInput;
            this.mainWindow = mainWindow;
            //MenuItems.ItemsSource = mainWindow.appData.Products;
            //MenuItems.DisplayMemberPath = "Name";
            List<string> typeOptions = new List<string>
            {
                "Приват",
                "Пиво",
                "Коктейль",
                "Шот",
                "Безалкогольное",
                "Подарок",
                "Special"
            };
            ItemType.ItemsSource = typeOptions;
        }
 

        private void DeleteReceipts_Click(object sender, RoutedEventArgs e)
        {
            decimal minimalPrice = Convert.ToDecimal(DeleteReceiptsPrice.Text);
            string path = "Receipts\\" + receiptsWindow.selectedMonth + "\\" + receiptsWindow.selectedDay;
            int i = 1;
            string curTrashJsonPath = MainWindow.trashBinPath + "/A" + DateTime.Now.ToString("d") + "(" + i + ").json";
            while (File.Exists(curTrashJsonPath))
            {
                 i++;
                curTrashJsonPath = System.IO.Path.Combine(MainWindow.trashBinPath, "D" + DateTime.Now.ToString("d") + "(" + i + ").json");
            }
            List<int> receiptsId = new List<int>();
            string message = "";

            foreach(string file in Directory.GetFiles(path))
            {
                Receipt receipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(file));
                if(receipt.PaymentType == 0 && receipt.TotalPrice >=  minimalPrice) 
                {
                    receiptsId.Add(receipt.Id);
                    message += receipt.Id + "\n";
                    string trashFileName = MainWindow.trashBinPath + "\\" + receipt.Id.ToString() + ".json";
                    File.Move(file, trashFileName);
                }
            }
            File.WriteAllText(curTrashJsonPath, JsonConvert.SerializeObject(receiptsId));
            MessageBox.Show(message);

            receiptsWindow.UpdateSelectedReceiptFolder();
        }

        private void MenuItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(MenuItems.SelectedIndex != -1)
            {
                Product selectedProduct = MenuItems.SelectedItem as Product;
                ItemName.Text = selectedProduct.Name;
                switch (selectedProduct.Type)
                {
                    case "privaat":
                        ItemType.SelectedItem = "Приват";
                        break;
                    case "beer":
                        ItemType.SelectedItem = "Пиво";
                        break;
                    case "cocktail":
                        ItemType.SelectedItem = "Коктейль";
                        break;
                    case "shot":
                        ItemType.SelectedItem = "Шот";
                        break;
                    case "alcofree":
                        ItemType.SelectedItem = "Безалкогольное";
                        break;
                    case "gift":
                        ItemType.SelectedItem = "Подарок";
                        break;
                    case "special":
                        ItemType.SelectedItem = "Special";
                        break;
                }
                ItemPrice.Text = selectedProduct.Price.ToString();
                Index.Text = mainWindow.appData.Products.IndexOf(selectedProduct).ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeProduct_Click(object sender, RoutedEventArgs e)
        {
            Product selectedProduct = (Product)MenuItems.SelectedItem;
            selectedProduct.Name = ItemName.Text;
            switch (ItemType.SelectedItem)
            {
                case "Приват":
                    selectedProduct.Type = "privaat";
                    break;
                case "Пиво":
                    selectedProduct.Type = "beer";
                    break;
                case "Коктейль":
                    selectedProduct.Type = "cocktail";
                    break;
                case "Шот":
                    selectedProduct.Type = "shot";
                    break;
                case "Безалкогольное":
                    selectedProduct.Type = "alcofree";
                    break;
                case "Подарок":
                    selectedProduct.Type = "gift";
                    break;
                case "Special":
                    selectedProduct.Type = "special";
                    break;
            }
            selectedProduct.Price = decimal.Parse(ItemPrice.Text);
            int index = mainWindow.appData.Products.IndexOf(selectedProduct);
            mainWindow.appData.Products[index] = selectedProduct;
            AppData.SaveAppData(mainWindow.appData);
            mainWindow.UpdateAppData();
        }

        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
