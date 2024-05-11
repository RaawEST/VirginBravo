using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup.Localizer;
using System.Diagnostics;
using System.Reflection;
#pragma warning disable
namespace VirginBravo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Импорт функций из printer.sdk.dll

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern IntPtr InitPrinter(string model);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int OpenPort(IntPtr intPtr, string port);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int ReleasePrinter(IntPtr intPtr);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int ClosePort(IntPtr intPtr);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int PrintText(IntPtr intPtr, string data, int alignment, int textSize);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int FeedLine(IntPtr intPtr, int lines);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int CutPaper(IntPtr intPtr, int cutMode);

        [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int OpenCashDrawer(IntPtr intPtr, int pinMode, int onTime, int ofTime);
        #endregion

        public static IntPtr printer;
        public static int openStatus;

        public static string defIp = $"NET,192.168.1.31";
        public static string currentDailyReceiptFolder;
        public static string trashBinPath = "Receipts/Trashbin";

        decimal currentTax = 0.22M;
        decimal orderPrice = 0;
        decimal discount = 0;
        public AppData appData = AppData.Instance;
        public static ObservableCollection<Product> productsInCurMenu = new ObservableCollection<Product>();
        public ObservableCollection<Product> productsInCurOrder { get; } = new ObservableCollection<Product>();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //CreateDefaultMenu();
            UpdateAppData();
            CreateDirectories();
            LBMenu.ItemsSource = productsInCurMenu;
            LBMenu.DisplayMemberPath = "Name";
        }

        private void PTchanged(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            string bttype = radioButton.Name.ToString();
            string type = bttype;

            productsInCurMenu.Clear();
            foreach (Product p in AppData.Instance.Products)
            {
                if (p.Type == type)
                    productsInCurMenu.Add(p);
            }

            RadioButton clickedButton = sender as RadioButton;

            // Перебрать все кнопки в одной группе (предполагается, что они в одной группе по GroupName)
            foreach (var rButton in PTstack.Children.OfType<RadioButton>())
            {
                // Если текущая кнопка не нажата, сбросить её состояние
                if (radioButton != clickedButton)
                {
                    radioButton.IsChecked = false;
                }
            }
        }

        private void UpdateOrderPrice()
        {
            //TBPrice.Text = orderPrice.ToString();
            orderPrice = 0;
            foreach (Product p in productsInCurOrder)
            {
                orderPrice += p.Price * p.QuantitySelled * (1 - discount);
            }
            TBPrice.Text = orderPrice.ToString();

        }
        /*private void CreateDefaultMenu()
        {
            //Классификация типов
             * 0 = Private
             * 1 = Beer
             * 2 = Cocktail
             * 3 = Cognac
             * 4 = Shots
             * 5 = Alco Free
             * 6 = Gifts
             * 7 = Special Drinks
             * 8 = Other
             * 9 = Passage
             
            if (!File.Exists(menuPath))
            {
                List<Product> defProducts = new List<Product> {
                    //private
                    new Product("Privaat 10 min", 0, 50),
                    new Product("Privaat 20 min", 0, 90),
                    new Product("Privaat 30 min", 0, 130),
                    new Product("Privaat 40 min", 0, 170),
                    new Product("Privaat 50 min", 0, 210),
                    new Product("Privaat 60 min", 0, 250),
                    new Product("Priv 2m 10 min", 0, 140),
                    new Product("Priv 2m 20 min", 0, 220),
                    new Product("Priv 2m 30 min", 0, 300),
                    new Product("Priv 2m 40 min", 0, 380),
                    new Product("Priv 2m 50 min", 0, 460),
                    new Product("Priv 2m 60 min", 0, 540),
                    new Product("Sauna", 0, 350),
                    new Product("Sauna 2m", 0, 740),
                    //olu
                    new Product("Saku Orig", 1, 7),
                    new Product("Gin Long Drink", 1, 7),
                    new Product("Tuborg", 1, 6),
                    new Product("Blanc", 1, 6),
                    new Product("Somersby", 1, 6),
                    //kokteil
                    new Product("Viski", 2, 10),
                    new Product("Gin Toonik", 2, 10),
                    new Product("Energia Viin", 2, 10),
                    new Product("Viin Mahlaga", 2, 10),
                    new Product("RummCola", 2, 10),
                    new Product("ViskiCola", 2, 10),
                    new Product("Vein Punane", 2, 10),
                    new Product("Vein Valge", 2, 10),
                    //konjak
                    new Product("Hennesy", 3, 15),
                    new Product("Larsen", 3, 15),
                    new Product("Courvoiseir", 3, 15),
                    //shotid
                    new Product("JagerMeister", 4, 5),
                    new Product("Tequila", 4, 5),
                    new Product("Viin", 4, 5),
                    new Product("Sambuca", 4, 5),
                    new Product("B52", 4, 5),
                    //alkovaba
                    new Product("Carlsberg", 5, 5),
                    new Product("Go Pale Ale", 5, 5),
                    new Product("Red Bull", 5, 5),
                    new Product("CocaCola", 5, 5),
                    new Product("CocaColaZ", 5, 5),
                    new Product("Kohv", 5, 5),
                    new Product("Tee", 5, 5),
                    new Product("Mahl", 5, 5),
                    new Product("Vesi", 5, 5),
                    //tudrukule
                    new Product("Lady Drink", 6, 25),
                    new Product("Prossecco", 6, 150),
                    new Product("Punane viin", 6, 150),
                    new Product("Valge viin", 6, 150),
                    new Product("M&C Brut Imp.", 6, 250),
                    new Product("Veuve Clicquot", 6, 300),
                    //Special Drinks
                    new Product("Fire Sambuca", 7, 25),
                    new Product("Lamborghini", 7, 25),
                    //Other
                    new Product("Shower Show", 7, 150),
                    new Product("Body Shot", 7, 20),
                    new Product("Lava Tants", 7, 65),
                    new Product("Topless seltskond", 7, 200),
                    new Product("Paljas seltskond", 7, 700),
                    new Product("Kasutatud aluspukse", 7, 215),
                    new Product("MF talks(1 k.)", 7, 500),
                    new Product("MF talks(koik)", 7, 1500),
                    new Product("Pidu parast 06:00", 7, 300),
                    //Passage, V-Bucks
                    new Product("Sissepaas", 8, 10),
                    new Product("V-Dollar", 8, 5)
                };
                Product.Save(defProducts, menuPath);
            }
        }*/

        public void UpdateAppData()
        {
            appData.GetAppData();
            //appData = appData.GetAppData();
            //FastProduct1.Content = appData.FastProduct1;
            //FastProduct2.Content = appData.FastProduct2;
            //FastProduct3.Content = appData.FastProduct3;
            //FastProduct4.Content = appData.FastProduct4;
            //FastProduct5.Content = appData.FastProduct5;
            //AppData.SaveAppData(appData);
        }

        public void ClearOrder()
        {
            productsInCurOrder.Clear();
            UpdateOrderPrice();
        }
        private void LBMenu_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            Product product = LBMenu.SelectedItem as Product;
            if (LBMenu.SelectedIndex != -1)
            {
                var existingProduct = productsInCurOrder.FirstOrDefault(p => p.Name == product.Name);
                if (existingProduct != null)
                {
                    existingProduct.QuantitySelled++;
                }
                else
                {
                    Product newProduct = product;
                    newProduct.QuantitySelled = 1;
                    productsInCurOrder.Add(newProduct);
                }
                UpdateOrderPrice();
            }


        }
        private void BTNDrawer_Click(object sender, RoutedEventArgs e)
        {
            printer = InitPrinter("");
            openStatus = OpenPort(printer, defIp);
            if (openStatus == 0)
                OpenCashDrawer(printer, 0, 30, 255);
            ReleasePrinter(printer);
            ClosePort(printer);
            openStatus = 100;
        }
        private void BTNCash_Click(object sender, RoutedEventArgs e)
        {
            PrintReceipt(0);
        }
        private void BTNCard_Click(object sender, RoutedEventArgs e)
        {
            PrintReceipt(1);
        }
        private void PrintReceipt(int paymentType)
        {
            {
                if (productsInCurOrder != null)
                {
                    appData.LastReceiptId += 1;
                    AppData.SaveAppData(appData);
                    List<Product> pList = new List<Product>();
                    foreach (Product product in productsInCurOrder)
                    {
                        pList.Add((Product)product.Clone());
                    }
                    Receipt newReceipt = new Receipt(appData.LastReceiptId, paymentType, pList, discount, currentTax);
                    Receipt.Save(newReceipt, currentDailyReceiptFolder);
                    Receipt.PrintReceipt(newReceipt, 0);

                }
                ClearOrder();
                UpdateOrderPrice();
            }
        }
        private void CreateDirectories()
        {
            DateTime currentTime = DateTime.Now;
            DateTime targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 16, 0, 0);

            if (!Directory.Exists("Receipts"))
                Directory.CreateDirectory("Receipts");
            if (!Directory.Exists(trashBinPath))
                Directory.CreateDirectory(trashBinPath);


            if (currentTime > targetTime)
            {
                currentDailyReceiptFolder = "Receipts\\" + currentTime.ToString("yyyy.MM") + "\\" + currentTime.ToString("dd");

                if (!Directory.Exists(currentDailyReceiptFolder))
                    Directory.CreateDirectory(currentDailyReceiptFolder);
            }
            if (currentTime < targetTime)
            {
                currentTime = currentTime.AddDays(-1);
                currentDailyReceiptFolder = System.IO.Path.Combine("Receipts\\" + currentTime.ToString("yyyy.MM") + "\\" + currentTime.ToString("dd"));

                if (!Directory.Exists(currentDailyReceiptFolder))
                    Directory.CreateDirectory(currentDailyReceiptFolder);
            }
        }

        private void BTNReceiptsWindow_Click(object sender, RoutedEventArgs e)
        {
            ReceiptsWindow newWindow = new ReceiptsWindow(this);
            newWindow.Show();
        }

        private void FastProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string btnContent = button.Content.ToString();
            foreach (Product product in appData.Products)
            {
                if (product.Name == btnContent)
                {
                    var existingProduct = productsInCurOrder.FirstOrDefault(p => p.Name == product.Name);
                    if (existingProduct != null)
                    {
                        existingProduct.QuantitySelled++;
                    }
                    else
                    {
                        Product newProduct = product;
                        newProduct.QuantitySelled = 1;
                        productsInCurOrder.Add(newProduct);
                    }
                    UpdateOrderPrice();
                    break;
                }
            }
        }
        private void FastPriv_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string btnContent = button.Content.ToString();
            string productToSearch = "Privaat 10 min";
            switch (btnContent)
            {
                case "10":
                    productToSearch = "Privaat 10 min";
                    break;
                case "20":
                    productToSearch = "Privaat 20 min";
                    break;
                case "30":
                    productToSearch = "Privaat 30 min";
                    break;
            }
            Product product = appData.Products.FirstOrDefault(p => p.Name == productToSearch);

            var existingProduct = productsInCurOrder.FirstOrDefault(p => p.Name == product.Name);
            if (existingProduct != null)
            {
                existingProduct.QuantitySelled++;
            }
            else
            {
                Product newProduct = product;
                newProduct.QuantitySelled = 1;
                productsInCurOrder.Add(newProduct);
            }
            UpdateOrderPrice();
        }

        private void TestButton(object sender, RoutedEventArgs e)
        {
            string message = "";
            int paymentType = 0;
            appData.LastReceiptId += 1;
            AppData.SaveAppData(appData);
            List<Product> pList = new List<Product>();
            foreach (Product product in productsInCurOrder)
            {
                pList.Add((Product)product.Clone());
            }
            Receipt newReceipt = new Receipt(appData.LastReceiptId, paymentType, pList, discount, currentTax);
            Receipt.Save(newReceipt, currentDailyReceiptFolder);
            foreach (Product item in newReceipt.Products)
            {
                if (item.Type != "privaat")
                {
                    message += item.Name + "=" + item.QuantitySelled + " = " + item.Price * item.QuantitySelled + "\n";
                }
                else
                {
                    message += item.Name + "=" + newReceipt.PrivateQuantity + " = " + newReceipt.PrivatePrice + "\n";
                }
            }
            decimal tax = 0.22M;
            message += "Receipt ID: " + newReceipt.Id + "\nDiscount: " + newReceipt.Discount + "\nTotalPrice: " + newReceipt.TotalPrice + "\nTotalPriceNoTax: " + newReceipt.TotalPriceNoTax + "\nTotalTax: " + newReceipt.TotalTax;

            MessageBox.Show(message);
            ClearOrder();

        }



        private void DelFromOrder_Click(object sender, RoutedEventArgs e)
        {
            if (LBOrder.SelectedIndex != -1)
            {
                productsInCurOrder[LBOrder.SelectedIndex].QuantitySelled -= 1;
                if (productsInCurOrder[LBOrder.SelectedIndex].QuantitySelled == 0)
                    productsInCurOrder.RemoveAt(LBOrder.SelectedIndex);
                UpdateOrderPrice();
            }
        }

        private void Multiplier(object sender, RoutedEventArgs e)
        {
            if (LBOrder.SelectedIndex != -1)
            {
                if (productsInCurOrder[LBOrder.SelectedIndex].QuantitySelled < 5)
                    productsInCurOrder[LBOrder.SelectedIndex].QuantitySelled = 5;
                else
                    productsInCurOrder[LBOrder.SelectedIndex].QuantitySelled += 5;
                UpdateOrderPrice();
            }
        }

        private void ClearOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            ClearOrder();
        }
        private void FastProduct(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string productName = button.Tag as string;
            Product newProduct = appData.Products.FirstOrDefault(p => p.Name == productName);
            if (newProduct != null)
            {
                var existingProduct = productsInCurOrder.FirstOrDefault(p => p.Name == newProduct.Name);
                if (existingProduct != null)
                {
                    existingProduct.QuantitySelled++;
                }
                else
                {
                    Product newProduct1 = newProduct;
                    newProduct.QuantitySelled = 1;
                    productsInCurOrder.Add(newProduct1);
                }
                UpdateOrderPrice();
            }
        }

        private void Discount_Checked(object sender, RoutedEventArgs e)
        {
            discount = 0.5M;
            UpdateOrderPrice();
        }

        private void Discount_Unchecked(object sender, RoutedEventArgs e)
        {
            discount = 0;
            UpdateOrderPrice();
        }
    }


}
