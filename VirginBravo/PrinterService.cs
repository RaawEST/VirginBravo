using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup.Localizer;

namespace VirginBravo
{
    internal class PrinterService
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

        private static IntPtr printer;
        private static int openStatus;
        private static string defIp = $"NET,192.168.1.31";
        static string dashes = new string('-', 32);

        public static void InitializePrinter()
        {
            printer = InitPrinter("");
            openStatus = OpenPort(printer, defIp);
        }
        public static void ShutdownPrinter()
        {
            ReleasePrinter(printer);
            ClosePort(printer);
            openStatus = 100;
        }

        public static void PrintSingleReceipt(Receipt receipt)
        {
            InitializePrinter();

            if (openStatus == 0)
            {
                PrintHeader();
                PrintHatSingleReceipt(receipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintText(printer, "Nimetus" + GetSpaces("Nimetus", "Summa") + "Summa\r\n", 0, 0);
                PrintProductsSingleReceipt(receipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintSingleReceiptInfo(receipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintSingleReceiptSum(receipt);
                FeedLine(printer, 5);
                CutPaper(printer, 0);
                ShutdownPrinter();
            }
            else if (openStatus != 0)
            {
                MessageBox.Show(Convert.ToString(PrintText(printer, "V BAAR\r\n", 1, 0)));
                ShutdownPrinter();
            }
        }
        public static void PrintTotalReceipt(TotalReceipt receipt, string typeOfReceipt)
        {
            InitializePrinter();
            if(openStatus == 0)
            {
                PrintHeader();
                PrintHatTotalReceipt(receipt, typeOfReceipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintText(printer, "Nimetus" + GetSpaces("Nimetus", "Summa") + "Summa\r\n", 0, 0);
                PrintProductsTotalReceipt(receipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintTotalReceiptInfo(receipt);
                PrintText(printer, dashes + "\r\n", 0, 0);
                PrintTotalReceiptSum(receipt);
                FeedLine(printer, 5);
                CutPaper(printer, 0);
                ShutdownPrinter();
            }
        }


        public static void TestPrintSingleReceipt(Receipt receipt)
        {
            string message = "V BAAR\n";
            message += "Tartu mnt 29, Tallinn\n";
            message += "Julounge OÜ\n";
            message += "Reg.Nr. 16712174\n";
            message += "Kuupaev: " + DateTime.Now.ToString() + "\n";
            message += "Kviitungi Nr: " + receipt.Id + "\n";
            foreach (Product item in receipt.Products)
            {
                if (item.Type != "privaat")
                {
                    message += item.Name + " " + item.QuantitySelled + GetSpaces(item.Name + " " + item.QuantitySelled, Math.Round(item.Price * item.QuantitySelled * (1 - receipt.Discount)).ToString()) + Math.Round(item.Price * item.QuantitySelled * (1 - receipt.Discount)) + "\n";
                }
                else
                {
                    message += item.Name + " " + receipt.PrivateQuantity + GetSpaces(item.Name + " " + receipt.PrivateQuantity, Math.Round(receipt.PrivatePrice * (1 - receipt.Discount)).ToString()) + Math.Round(receipt.PrivatePrice * (1 - receipt.Discount)) + "\n";
                }
            }
            if (receipt.PaymentType == 0)
                message += "Sularaha" + "\n";
            else
                message += "Kaardimakse" + "\n";
            if (receipt.Discount != 0)
            {
                message += "Allahindlus " + receipt.Discount * 100 + "%" + "\n";
            }
            message += "Kokku KM-ta" + GetSpaces("Kokku KM-ta", Convert.ToString(Math.Round((receipt.TotalPriceNoTax), 2))) + Math.Round((receipt.TotalPriceNoTax), 2) + "\n";
            message += "22% KM" + GetSpaces("22% KM", Convert.ToString(Math.Round((receipt.TotalTax), 2))) + Math.Round((receipt.TotalTax), 2) + "\n";
            message += "Kokku KM-ga" + GetSpaces("Kokku KM-ga", Math.Round((receipt.TotalPrice), 2).ToString()) + Math.Round((receipt.TotalPrice), 2) + "\n";
            MessageBox.Show(message);
        }
        public static void TestPrintTotalReceipt(TotalReceipt receipt, string typeOfReceipt)
        {
            string message = "V BAAR\n";
            message += "Tartu mnt 29, Tallinn\n";
            message += "Julounge OÜ\n";
            message += "Reg.Nr. 16712174\n";
            string type = "Tulemus\r\n";
            if (typeOfReceipt == "daily")
                type = "Paevatulemus\r\n";
            if (typeOfReceipt == "monthly")
                type = "Kuutulemus\r\n";
            message += type;
            message += "Kuupaev: " + receipt.Date + "\n";
            foreach(Product item in receipt.Products)
            {
                if (item.Type != "privaat")
                    message += item.Name + " " + item.QuantitySelled + GetSpaces(item.Name + " " + item.QuantitySelled, Math.Round(item.Price).ToString()) + Math.Round(item.Price) + "\r\n";
                else
                    message += item.Name + " " + receipt.PrivateQuantity + GetSpaces(item.Name + " " + receipt.PrivateQuantity, Math.Round(receipt.PrivatePrice).ToString()) + Math.Round(receipt.PrivatePrice) + "\r\n";
            }
            message += "Cash: " + receipt.Cash + "\n";
            message += "Card: " + receipt.Card + "\n";
            message += "Kokku KM-ta" + GetSpaces("Kokku KM-ta", Convert.ToString(Math.Round((receipt.TotalPriceNoTax), 2))) + Math.Round((receipt.TotalPriceNoTax), 2) + "\n";
            message += "22% KM" + GetSpaces("22% KM", Convert.ToString(Math.Round((receipt.TotalTax), 2))) + Math.Round((receipt.TotalTax), 2) + "\n";
            message += "Kokku KM-ga" + GetSpaces("Kokku KM-ga", Math.Round((receipt.TotalPrice), 2).ToString()) + Math.Round((receipt.TotalPrice), 2) + "\n";
            MessageBox.Show(message);
        }


        public static void PrintHeader()
        {
            PrintText(printer, "V BAAR\r\n", 1, 0);
            PrintText(printer, "Tartu mnt 29, Tallinn\r\n", 1, 0);
            PrintText(printer, "Julounge OÜ\r\n", 1, 0);
            PrintText(printer, "Reg.Nr. 16712174\r\n", 1, 0);
        }
        public static void PrintHatSingleReceipt(Receipt receipt)
        {
            PrintText(printer, "Kuupaev: " + DateTime.Now.ToString() + "\r\n", 0, 0);
            PrintText(printer, "Kviitungi Nr: " + receipt.Id + "\r\n", 0, 0);
        }
        public static void PrintHatTotalReceipt(TotalReceipt receipt, string typeOfReceipt)
        {
            string type = "Tulemus\r\n";
            if (typeOfReceipt == "daily")
                type = "Paevatulemus\r\n";
            if (typeOfReceipt == "monthly")
                type = "Kuutulemus\r\n";
            PrintText(printer, type, 1, 1);
            PrintText(printer, "Kuupaev: " + receipt.Date + "\r\n", 0, 0);
        }
        public static void PrintProductsSingleReceipt(Receipt receipt)
        {
            foreach (Product item in receipt.Products)
            {
                if (item.Type != "privaat")
                {
                    PrintText(printer, item.Name + " " + item.QuantitySelled + GetSpaces(item.Name + " " + item.QuantitySelled, Math.Round(item.Price * item.QuantitySelled * (1 - receipt.Discount)).ToString()) + Math.Round(item.Price * item.QuantitySelled * (1 - receipt.Discount)) + "\r\n", 0, 0);
                }
                else
                {
                    PrintText(printer, item.Name + " " + receipt.PrivateQuantity + GetSpaces(item.Name + " " + receipt.PrivateQuantity, Math.Round(receipt.PrivatePrice * (1 - receipt.Discount)).ToString()) + Math.Round(receipt.PrivatePrice * (1 - receipt.Discount)) + "\r\n", 0, 0);
                }
            }
        }
        public static void PrintProductsTotalReceipt(TotalReceipt receipt)
        {
            foreach (Product item in receipt.Products)
            {
                if (item.Type != "privaat")
                {
                    PrintText(printer, item.Name + " " + item.QuantitySelled + GetSpaces(item.Name + " " + item.QuantitySelled, Math.Round(item.Price).ToString()) + Math.Round(item.Price) + "\r\n", 0, 0);
                }
                else
                {
                    PrintText(printer, item.Name + " " + receipt.PrivateQuantity + GetSpaces(item.Name + " " + receipt.PrivateQuantity, Math.Round(receipt.PrivatePrice).ToString()) + Math.Round(receipt.PrivatePrice) + "\r\n", 0, 0);
                }
            }
        }
        public static void PrintSingleReceiptInfo(Receipt receipt)
        {
            if (receipt.PaymentType == 0)
                PrintText(printer, "Sularaha\r\n", 0, 0);
            else
                PrintText(printer, "Kaardimakse\r\n", 0, 0);
            if (receipt.Discount != 0)
            {
                PrintText(printer, "Allahindlus " + receipt.Discount * 100 + "%" + "\r\n", 0, 0);
            }
        }
        public static void PrintTotalReceiptInfo(TotalReceipt receipt)
        {
            PrintText(printer, "Cash: " + receipt.Cash + "\r\n", 0, 0);
            PrintText(printer, "Card: " + receipt.Card + "\r\n", 0, 0);
        }
        public static void PrintSingleReceiptSum(Receipt receipt)
        {
            PrintText(printer, "Kokku KM-ta" + GetSpaces("Kokku KM-ta", Convert.ToString(Math.Round((receipt.TotalPriceNoTax), 2))) + Math.Round((receipt.TotalPriceNoTax), 2) + "\r\n", 0, 1);
            PrintText(printer, "22% KM" + GetSpaces("22% KM", Convert.ToString(Math.Round((receipt.TotalTax), 2))) + Math.Round((receipt.TotalTax), 2) + "\r\n", 0, 0);
            PrintText(printer, "Kokku KM-ga" + GetSpaces("Kokku KM-ga", Math.Round((receipt.TotalPrice), 2).ToString()) + Math.Round((receipt.TotalPrice), 2) + "\r\n", 0, 1);
        }
        public static void PrintTotalReceiptSum(TotalReceipt receipt)
        {
            PrintText(printer, "Kokku KM-ta" + GetSpaces("Kokku KM-ta", Convert.ToString(Math.Round((receipt.TotalPriceNoTax), 2))) + Math.Round((receipt.TotalPriceNoTax), 2) + "\r\n", 0, 1);
            PrintText(printer, "22% KM" + GetSpaces("22% KM", Convert.ToString(Math.Round((receipt.TotalTax), 2))) + Math.Round((receipt.TotalTax), 2) + "\r\n", 0, 0);
            PrintText(printer, "Kokku KM-ga" + GetSpaces("Kokku KM-ga", Math.Round((receipt.TotalPrice), 2).ToString()) + Math.Round((receipt.TotalPrice), 2) + "\r\n", 0, 1);
        }

        private static string GetSpaces(string fString, string sString)
        {
            int spacesCount = 32 - fString.Length - sString.Length;
            string spaces = new string(' ', spacesCount);
            return spaces;
        }
    }
}
