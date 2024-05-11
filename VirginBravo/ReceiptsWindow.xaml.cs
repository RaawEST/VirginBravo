using Newtonsoft.Json;
using Simplified;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml.XPath;
#pragma warning disable
namespace VirginBravo
{
    /// <summary>
    /// Логика взаимодействия для ReceiptsWindow.xaml
    /// </summary>
    public partial class ReceiptsWindow : Window
    {
        MainWindow mainWindow;
        public ReceiptsWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            //CheckReceiptsFolder();
            //DetectCurDay();
            LBReceipts.ItemsSource = receiptsInFolder;
            LBReceiptContainer.ItemsSource = productsInSelectedReceipt;
        }
        public string selectedMonth;
        public string selectedDay;
        public ObservableCollection<Receipt> receiptsInFolder = new ObservableCollection<Receipt>();
        ObservableCollection<Product> productsInSelectedReceipt = new ObservableCollection<Product>();
        private ReceiptsViewModel receiptsVM = new ReceiptsViewModel();



        /*private void CheckReceiptsFolder()
        {
            LBMonth.Items.Clear();
            if (Directory.Exists("Receipts\\"))
            {
                string[] DRfolder = Directory.GetDirectories("Receipts\\");
                foreach (string folder in DRfolder)
                {
                    LBMonth.Items.Add(folder.Substring(9));
                }
            }
        }*/
        private void DetectCurDay()
        {
            DateTime currentTime = DateTime.Now;
            DateTime targetTime1 = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 16, 0, 0);

            if (currentTime < targetTime1)
            {
                currentTime = currentTime.AddDays(-1);
            }
            foreach (DateOnly month in receiptsVM.Months)
            {
                if (month.ToString("yyyy.MM") == currentTime.ToString("yyyy.MM"))
                {
                    LBMonth.SelectedItem = month;
                    foreach (DateOnly day in receiptsVM.Days)
                    {
                        if (day.ToString("dd") == currentTime.ToString("dd"))
                        {
                            LBDay.SelectedItem = day.ToString("dd");
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void LBMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (LBMonth.SelectedIndex != -1)
            {
                receiptsVM.SelectedMonth = (DateOnly)LBMonth.SelectedItem;
            }*/
        }

        private void LBDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedReceiptFolder();
        }

        private void ReceiptUnselected()
        {
            SaveChanges.IsEnabled = false;
            BTNDelReceipt.IsEnabled = false;
            RBCash.IsChecked = false;
            RBCard.IsChecked = false;
            ReceiptIdTB.Clear();
            Sum.Text = "Сумма: ";
            productsInSelectedReceipt.Clear();
        }

        private void LBReceipts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBReceipts.SelectedIndex != -1)
            {
                productsInSelectedReceipt.Clear();
                Receipt selectedReceipt = receiptsInFolder[LBReceipts.SelectedIndex];
                foreach (Product item in selectedReceipt.Products)
                {
                    productsInSelectedReceipt.Add(item);
                }

                ReceiptIdTB.Text = selectedReceipt.Id.ToString();
                if (selectedReceipt.PaymentType == 0)
                    RBCash.IsChecked = true;
                else
                    RBCard.IsChecked = true;
                SaveChanges.IsEnabled = true;
                BTNDelReceipt.IsEnabled = true;
                Sum.Text = "Сумма: " + selectedReceipt.TotalPrice.ToString();
            }
            else
            {
                ReceiptUnselected();
            }
        }

        private void BTNSave_Click(object sender, RoutedEventArgs e)
        {
            Receipt receipt = receiptsInFolder[LBReceipts.SelectedIndex];
            if (RBCash.IsChecked == true)
                receiptsInFolder[LBReceipts.SelectedIndex].PaymentType = 0;
            else
                receiptsInFolder[LBReceipts.SelectedIndex].PaymentType = 1;

            File.WriteAllText("Receipts\\" + selectedMonth + "\\" + selectedDay + "\\" + receipt.OrderTime.ToString("HH.mm.ss") + ".json", JsonConvert.SerializeObject(receipt));

            //CheckReceiptsFolder();
            LBMonth.SelectedItem = receipt.OrderTime.Year + "." + receipt.OrderTime.Month;
            LBDay.SelectedItem = receipt.OrderTime.Day;
            LBReceipts.SelectedItem = receipt.OrderTime.ToString("HH:mm:ss");

            UpdateSelectedReceiptFolder();
            productsInSelectedReceipt.Clear();



            MessageBox.Show("Сохранено!");
        }
        public void UpdateSelectedReceiptFolder()
        {
            if (LBDay.SelectedIndex != -1)
            {
                selectedDay = LBDay.SelectedItem.ToString();
                receiptsInFolder.Clear();

                foreach (string item in Directory.GetFiles("Receipts\\" + selectedMonth + "\\" + selectedDay))
                {
                    Receipt json = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(item));
                    receiptsInFolder.Add(json);
                }
            }
        }
        private void PrintTotalReceipt(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            List<Receipt> receiptList = new List<Receipt>();
            string date = "";
            string args = "None";
            if (button.Name == "BTNDailyReceipt")
            {
                args = "daily";
                date = (selectedMonth + "." + selectedDay).ToString();
                foreach (string file in Directory.GetFiles("Receipts\\" + selectedMonth + "\\" + selectedDay))
                {
                    Receipt receipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(file));
                    receiptList.Add(receipt);
                }
            }
            else
            {
                args = "monthly";
                date = selectedMonth.ToString();
                foreach (string day in Directory.GetDirectories("Receipts\\" + selectedMonth))
                {
                    foreach (string file in Directory.GetFiles(day))
                    {
                        Receipt receipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(file));
                        receiptList.Add(receipt);
                    }
                }
            }
            TotalReceipt totalReceipt = new TotalReceipt(receiptList, date);
            PrinterService.PrintTotalReceipt(totalReceipt, args);
        }
        private void BTNReprint_Click(object sender, RoutedEventArgs e)
        {
            Receipt receipt = receiptsInFolder[LBReceipts.SelectedIndex];
            PrinterService.PrintSingleReceipt(receipt);
        }

        private void DelReceipt(object sender, RoutedEventArgs e)
        {
            File.Delete("Receipts\\" + selectedMonth + "\\" + selectedDay + "\\" + receiptsInFolder[LBReceipts.SelectedIndex].OrderTime.ToString("HH.mm.ss") + ".json");
            UpdateSelectedReceiptFolder();
            productsInSelectedReceipt.Clear();
        }

        public void NumericTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumeric(e.Text))
            {
                e.Handled = true; // Отменяем ввод, если текст не является числом
            }
        }
        private bool IsNumeric(string text)
        {
            // Проверяем, является ли текст числом
            return int.TryParse(text, out _);
        }



        private void ReceiptIdSearch_Click(object sender, RoutedEventArgs e)
        {
            string idToSearchString = ReceiptIdTB.Text;
            if (idToSearchString.Length < 5)
            {
                MessageBox.Show("ID неверный");
                return;
            }
            string monthOfReceipt = Convert.ToString(2000 + Convert.ToInt32(idToSearchString.Substring(0, 2))) + "." + idToSearchString.Substring(2, 2);
            if (Directory.Exists("Receipts\\" + monthOfReceipt))
            {
                LBMonth.SelectedItem = monthOfReceipt;
                foreach (string day in Directory.GetDirectories("Receipts\\" + monthOfReceipt))
                {
                    foreach (string receipt in Directory.GetFiles(day))
                    {
                        Receipt receiptJson = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(receipt));
                        if (receiptJson.Id == int.Parse(idToSearchString))
                        {
                            if (receiptJson.OrderTime.Hour < 16)
                                LBDay.SelectedItem = Convert.ToString(receiptJson.OrderTime.Day - 1);
                            else
                                LBDay.SelectedItem = receiptJson.OrderTime.Day.ToString();
                            foreach (Receipt receipt1 in receiptsInFolder)
                            {
                                if (receipt1.Id == int.Parse(idToSearchString))
                                {
                                    LBReceipts.SelectedItem = receipt1;
                                }
                            }
                            return;
                        }
                    }
                }
                MessageBox.Show("ID неверный");
                return;
            }
            else
            {
                MessageBox.Show("ID неверный");
            }
        }

        private void ButtonTestSingleReceipt_Click(object sender, RoutedEventArgs e)
        {
            PrinterService.TestPrintSingleReceipt(receiptsInFolder[LBReceipts.SelectedIndex]);
        }
        private void ButtonTestDailyReceipt_Click(object sender, RoutedEventArgs e)
        {
            List<Receipt> receiptList = new List<Receipt>();
            string date = (selectedMonth + "." + selectedDay).ToString();
            foreach (string file in Directory.GetFiles("Receipts\\" + selectedMonth + "\\" + selectedDay))
            {
                Receipt receipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(file));
                receiptList.Add(receipt);
            }
            TotalReceipt totalReceipt = new TotalReceipt(receiptList, date);
            PrinterService.TestPrintTotalReceipt(totalReceipt, "daily");
        }
        private void ButtonTestMonthlyReceipt_Click(object sender, RoutedEventArgs e)
        {
            List<Receipt> receiptList = new List<Receipt>();
            string date = selectedMonth.ToString();
            foreach (string day in Directory.GetDirectories("Receipts\\" + selectedMonth))
            {
                foreach (string file in Directory.GetFiles(day))
                {
                    Receipt receipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(file));
                    receiptList.Add(receipt);
                }
            }
            TotalReceipt totalReceipt = new TotalReceipt(receiptList, date);
            PrinterService.TestPrintTotalReceipt(totalReceipt, "monthly");
        }

        private void KGB_Click(object sender, RoutedEventArgs e)
        {
            KGBPassword kgbPassword = new KGBPassword(this, mainWindow);
            kgbPassword.Show();
        }
    }

    public class ReceiptsViewModel : BaseInpc
    {
        private DateOnly _selectedMonth;
        private DateOnly _selectedDay;

        public int Number { get; }
        private int count;

        public DateOnly SelectedMonth { get => _selectedMonth; set => Set(ref _selectedMonth, value); } //selectedMonth = value; }
        public ObservableCollection<DateOnly> Months { get; } = new();
        public DateOnly SelectedDay { get => _selectedDay; set => Set(ref _selectedDay, value); }
        public ObservableCollection<DateOnly> Days { get; } = new();
        public ObservableCollection<DateOnly> TestDays { get; } = new();

        public ReceiptsViewModel()
        {
            count++;
            Number = count;

            if (Directory.Exists("Receipts\\"))
            {
                string[] DRfolder = Directory.GetDirectories("Receipts\\");
                foreach (string folder in DRfolder)
                {
                    try
                    {
                        Months.Add(DateOnly.ParseExact(System.IO.Path.GetFileName(folder), "yyyy.MM"));
                    }
                    catch
                    {

                        continue;
                    }
                }
            }

            TestDays.Add(new DateOnly());

        }

        public static readonly string ReceiptsFolder = "Receipts";
        public static readonly string AppAssembly = Assembly.GetExecutingAssembly().Location;
        public static readonly string AppFolder = System.IO.Path.GetDirectoryName(AppAssembly);
        public static readonly string ReceiptsFullFolder = System.IO.Path.Combine(AppFolder, ReceiptsFolder);


        protected override void OnPropertyChanged(string? propertyName, object? oldValue, object? newValue)
        {
            base.OnPropertyChanged(propertyName, oldValue, newValue);

            if (propertyName == nameof(SelectedMonth))
            {
                Days.Clear();
                string[] daysInMonthFolder = Directory.GetDirectories(System.IO.Path.Combine(ReceiptsFullFolder, SelectedMonth.ToString("yyyy.MM")));
                foreach (string folder in daysInMonthFolder)
                {
                    Days.Add(DateOnly.ParseExact(System.IO.Path.GetFileName(folder), "dd"));
                }
            }
            if (propertyName == nameof(SelectedMonth))
            {
                TestDays.Clear();
                string[] daysInMonthFolder = Directory.GetDirectories(System.IO.Path.Combine(ReceiptsFullFolder, SelectedMonth.ToString("yyyy.MM")));
                foreach (string folder in daysInMonthFolder)
                {
                    int day = int.Parse(System.IO.Path.GetFileName(folder));
                    DateOnly date = SelectedMonth.AddDays(day-1);
                    TestDays.Add(date);
                }
            }

        }
    }
}

