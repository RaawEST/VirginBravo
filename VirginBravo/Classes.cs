using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace VirginBravo
{
    [Serializable]
    public class Product : ICloneable, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        private int quantity;
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public int QuantitySelled

        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }
        public decimal TotalPrice => Price * QuantitySelled;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Product(string name, string type, decimal price)
        {
            Name = name;
            Type = type;
            Price = price;
            QuantitySelled = 0;
        }
        public static void Save(List<Product> products, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(products));
        }

        public object Clone()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(memoryStream);
            }
        }
    }
    public class Receipt
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
        #endregion
        public DateTime OrderTime { get; set; }
        public int Id { get; set; }
        public List<Product> Products { get; set; }
        public int PaymentType { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalPriceNoTax { get; set; }
        public decimal PrivatePrice { get; set; }
        public int PrivateQuantity { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercent { get; set; }


        public Receipt()
        {
            // Инициализация свойств по умолчанию, если необходимо
        }

        public Receipt(int id, int paymentType, List<Product> products, decimal discount, decimal tax)
        {
            TaxPercent = tax;
            Discount = discount;
            OrderTime = DateTime.Now;
            Id = Convert.ToInt32((OrderTime.Year - 2000).ToString("D2") + OrderTime.Month.ToString("D2") + id.ToString());
            PaymentType = paymentType;
            PrivatePrice = 0;
            PrivateQuantity = 0;

            List<Product> productsToSave = new List<Product>();
            foreach (Product p in products)
            {
                if (p.Type == "privaat")
                {
                    Product existingProduct = productsToSave.FirstOrDefault(product => product.Type == p.Type);
                    if (existingProduct == null)
                        productsToSave.Add(new Product("Privaat", "privaat", 0));
                    PrivatePrice += p.Price * p.QuantitySelled;
                    TotalPrice += p.Price * p.QuantitySelled;
                    PrivateQuantity++;
                }
                else
                {
                    productsToSave.Add(p);
                    TotalPrice += p.Price * p.QuantitySelled;
                }
            }
            Products = productsToSave;
            TotalPrice *= 1 - Discount;
            TotalPriceNoTax = Math.Round(TotalPrice / (1 + TaxPercent), 2);
            TotalTax = TotalPrice - TotalPriceNoTax;
        }
        public static void Save(Receipt receipt, string path)
        {
            string pathname = path + "\\" + receipt.OrderTime.ToString("HH.mm.ss") + ".json";
            File.WriteAllText(pathname, JsonConvert.SerializeObject(receipt));
        }
        public static List<Receipt> LoadList(string path)
        {
            try
            {
                string[] rPaths = Directory.GetFiles(path);

                List<Receipt> receipts = new List<Receipt>();
                foreach (string rPath in rPaths)
                {
                    Receipt newReceipt = JsonConvert.DeserializeObject<Receipt>(File.ReadAllText(rPath));
                    receipts.Add(newReceipt);
                }
                return receipts;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex);
                return null;
            }
        }

        private static IntPtr printer;
        private static int openStatus;
        private static string defIp = $"NET,192.168.1.31";
        public static void PrintReceipt(Receipt receipt, int typeOfPrinting)
        {
            printer = InitPrinter("");
            openStatus = OpenPort(printer, defIp);
            string dashes = new string('-', 32);

            if (openStatus == 0)
            {
                PrintText(printer, "V BAAR\r\n", 1, 0);
                PrintText(printer, "Tartu mnt 29, Tallinn\r\n", 1, 0);
                PrintText(printer, "Julounge OÜ\r\n", 1, 0);
                PrintText(printer, "Reg.Nr. 16712174\r\n", 1, 0);
                FeedLine(printer, 1);
                PrintText(printer, "Kuupaev: " + DateTime.Now.ToString() + "\r\n", 0, 0);
                PrintText(printer, "Kviitungi Nr: " + receipt.Id + "\r\n", 0, 0);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintText(printer, "Nimetus" + GetSpaces("Nimetus", "Summa") + "Summa\r\n", 0, 0);

                foreach (Product item in receipt.Products)
                {
                    if (item.Type != "privaat")
                    {
                        PrintText(printer, item.Name + " " + item.QuantitySelled + GetSpaces(item.Name + " " + item.QuantitySelled, Math.Round(item.Price*item.QuantitySelled * (1-receipt.Discount)).ToString()) + Math.Round(item.Price * item.QuantitySelled * (1 - receipt.Discount)) + "\r\n", 0, 0);
                    }
                    else
                    {
                        PrintText(printer, item.Name + " " + receipt.PrivateQuantity + GetSpaces(item.Name + " " + receipt.PrivateQuantity, Math.Round(receipt.PrivatePrice * (1-receipt.Discount)).ToString()) + Math.Round(receipt.PrivatePrice * (1-receipt.Discount)) + "\r\n", 0, 0);
                    }
                }

                PrintText(printer, dashes + "\r\n", 0, 0);
                if (receipt.PaymentType == 0)
                    PrintText(printer, "Sularaha\r\n", 0, 0);
                else
                    PrintText(printer, "Kaardimakse\r\n", 0, 0);

                if(receipt.Discount != 0)
                {
                    PrintText(printer, "Allahindlus " + receipt.Discount*100 + "%" + "\r\n", 0, 0);
                }

                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintText(printer, "Kokku KM-ta" + GetSpaces("Kokku KM-ta", Convert.ToString(Math.Round((receipt.TotalPriceNoTax), 2))) + Math.Round((receipt.TotalPriceNoTax), 2) + "\r\n", 0, 1);
                PrintText(printer, "22% KM" + GetSpaces("22% KM", Convert.ToString(Math.Round((receipt.TotalTax), 2))) + Math.Round((receipt.TotalTax), 2) + "\r\n", 0, 0);
                PrintText(printer, "Kokku KM-ga" + GetSpaces("Kokku KM-ga", Math.Round((receipt.TotalPrice), 2).ToString()) + Math.Round((receipt.TotalPrice), 2) + "\r\n", 0, 1);
                FeedLine(printer, 5);
                CutPaper(printer, 0);
                ReleasePrinter(printer);
                ClosePort(printer);
                openStatus = 100;
            }
            else if (openStatus != 0)
            {
                MessageBox.Show(Convert.ToString(PrintText(printer, "V BAAR\r\n", 1, 0)));
                ReleasePrinter(printer);
                ClosePort(printer);
                openStatus = 100;
            }


        }
        private static string GetSpaces(string fString, string sString)
        {
            int spacesCount = 32 - fString.Length - sString.Length;
            string spaces = new string(' ', spacesCount);
            return spaces;
        }
    }
    public class TotalReceipt
    {
        public List<Product> Products { get; set; }
        public decimal Cash { get; set; }
        public decimal Card { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceNoTax { get; set; }
        public decimal TotalTax {  get; set; }
        private decimal TaxAverage { get; set; }
        public decimal PrivatePrice { get; set; }
        public int PrivateQuantity { get; set; }
        public string Date {  get; set; }

        public TotalReceipt(List<Receipt> receipts, string date)
        {
            TotalPrice = 0;
            TaxAverage = 0;
            Cash = 0;
            Card = 0;
            PrivatePrice = 0;
            PrivateQuantity = 0;
            Date = date;

            Products = new List<Product>();
            foreach (Receipt r in receipts)
            {
                foreach (Product p in r.Products)
                {

                    if (p.Type == "privaat")
                    {
                        Product existingProduct = Products.FirstOrDefault(product => product.Type == p.Type);
                        if (existingProduct == null)
                            Products.Add(new Product("Privaat", "privaat", 0));

                        PrivateQuantity++;
                    }
                    else
                    {
                        Product existingProduct = Products.FirstOrDefault(product => product.Name == p.Name);
                        if (existingProduct != null)
                        {
                            existingProduct.QuantitySelled += p.QuantitySelled;
                            existingProduct.Price += p.Price * (1 - r.Discount);
                        }
                        else
                        {
                            Product newProduct = p;
                            newProduct.Price *= (1 - r.Discount);
                            Products.Add(newProduct);
                        }
                    }
                }
                if (r.PaymentType == 0)
                    Cash += r.TotalPrice;
                else
                    Card += r.TotalPrice;
                PrivatePrice += r.PrivatePrice;
                TotalPrice = Cash + Card;
                TotalPriceNoTax = Math.Round(TotalPrice / (1 + r.TaxPercent), 2);
                TotalTax = TotalPrice - TotalPriceNoTax;
                TaxAverage += r.TaxPercent;
            }
            TaxAverage /= receipts.Count();
 
        }
    }
}
