using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FamilySupermarketPOS.Models;

namespace FamilySupermarketPOS.Services
{
    public class HardwareService
    {
        private static int _invoiceCounter = 10001;

        public static string GenerateNextInvoiceNumber()
        {
            return $"INV-{_invoiceCounter++}";
        }

        public static string GenerateReceiptSlip(
            string invoiceNo, 
            List<CartItem> items, 
            double grandTotal, 
            double tendered, 
            double change, 
            string paymentMethod, 
            string cashierName)
        {
            var sb = new StringBuilder();
            const int width = 42; // standard 80mm printer character width

            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine(CenterText("AHF PROPERTIES", width));
            sb.AppendLine(CenterText("T/A FAMILY SUPERMARKET", width));
            sb.AppendLine(CenterText("160 MAIN REEF ROAD, JHB", width));
            sb.AppendLine(CenterText("TEL: (011) 830-2722", width));
            sb.AppendLine(CenterText("VAT NO: 4710192837", width));
            sb.AppendLine("".PadRight(width, '='));

            sb.AppendLine($"Date:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Invoice: {invoiceNo}");
            sb.AppendLine($"Cashier: {cashierName} (M1)");
            sb.AppendLine($"Account: CASH (Cash Sale)");
            sb.AppendLine("".PadRight(width, '-'));

            sb.AppendLine("ITEM                 QTY   PRICE     TOTAL");
            sb.AppendLine("".PadRight(width, '-'));

            foreach (var item in items)
            {
                string descr = item.Description.Length > 18 ? item.Description[..18] : item.Description;
                string line = $"{descr,-19} {item.Qty,3} {item.TotalIncl / item.Qty,7:N2} {item.TotalIncl,9:N2}";
                sb.AppendLine(line);
            }

            double vatRate = 0.15;
            double subTotal = Math.Round(grandTotal / (1 + vatRate), 2);
            double vat = Math.Round(grandTotal - subTotal, 2);

            sb.AppendLine("".PadRight(width, '-'));
            sb.AppendLine($"Subtotal (Excl VAT):           R {subTotal,9:N2}");
            sb.AppendLine($"VAT @ 15%:                     R {vat,9:N2}");
            sb.AppendLine($"TOTAL DUE (INCL):              R {grandTotal,9:N2}");
            sb.AppendLine("".PadRight(width, '='));

            sb.AppendLine($"Payment Method: {paymentMethod}");
            if (paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"Cash Tendered:                 R {tendered,9:N2}");
                sb.AppendLine($"CHANGE:                        R {change,9:N2}");
            }
            sb.AppendLine("".PadRight(width, '='));

            sb.AppendLine(CenterText("THANK YOU FOR SHOPPING WITH US", width));
            sb.AppendLine(CenterText("PLEASE RETAIN SLIP FOR RETURNS", width));
            sb.AppendLine(CenterText("*** REPRINT / AUDIT COPY ***", width));
            sb.AppendLine("".PadRight(width, '='));

            return sb.ToString();
        }

        public static void SaveReceiptToFile(string invoiceNo, string receiptText)
        {
            try
            {
                string receiptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(receiptsFolder);
                string filePath = Path.Combine(receiptsFolder, $"{invoiceNo}.txt");
                File.WriteAllText(filePath, receiptText);
            }
            catch
            {
                // Non-critical
            }
        }

        private static string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int leftPadding = (width - text.Length) / 2;
            return text.PadLeft(leftPadding + text.Length).PadRight(width);
        }
    }
}
