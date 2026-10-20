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
        private static int _creditNoteCounter = 5001;
        public static string LastReceiptText { get; private set; } = "";
        public static string LastInvoiceNumber { get; private set; } = "";

        public static string GenerateNextInvoiceNumber()
        {
            return $"BK-{_invoiceCounter++}";
        }

        public static string GenerateNextCreditNoteNumber()
        {
            return $"CN-{_creditNoteCounter++}";
        }

        public static string GenerateReceiptSlip(
            string invoiceNo, 
            List<CartItem> items, 
            double grandTotal, 
            double tendered, 
            double change, 
            string paymentMethod, 
            string cashierName,
            string accountName = "CASH (Cash Sale)")
        {
            var sb = new StringBuilder();
            const int width = 42;

            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine(CenterText("BLUE_KNIGHT SUPERMARKET", width));
            sb.AppendLine(CenterText("POINT OF SALE & RETAIL", width));
            sb.AppendLine(CenterText("CAPE TOWN / JHB", width));
            sb.AppendLine(CenterText("VAT NO: 4710192837", width));
            sb.AppendLine("".PadRight(width, '='));

            sb.AppendLine($"Date:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Invoice: {invoiceNo}");
            sb.AppendLine($"Cashier: {cashierName} (M1)");
            sb.AppendLine($"Account: {accountName}");
            sb.AppendLine("".PadRight(width, '-'));

            sb.AppendLine("ITEM                 QTY   PRICE     TOTAL");
            sb.AppendLine("".PadRight(width, '-'));

            foreach (var item in items)
            {
                string descr = item.Description.Length > 18 ? item.Description[..18] : item.Description;
                double unitPrice = item.Qty != 0 ? item.TotalIncl / item.Qty : item.PriceExcl * 1.15;
                string line = $"{descr,-19} {item.Qty,3} {unitPrice,7:N2} {item.TotalIncl,9:N2}";
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
            sb.AppendLine(CenterText("BLUE_KNIGHT RETAIL SYSTEMS", width));
            sb.AppendLine("".PadRight(width, '='));

            LastReceiptText = sb.ToString();
            LastInvoiceNumber = invoiceNo;

            return LastReceiptText;
        }

        public static string GenerateCreditNoteSlip(
            string creditNoteNo,
            List<CartItem> items,
            double refundTotal,
            string cashierName,
            string accountName = "CASH (Cash Sale)")
        {
            var sb = new StringBuilder();
            const int width = 42;

            sb.AppendLine("".PadRight(width, '*'));
            sb.AppendLine(CenterText("BLUE_KNIGHT SUPERMARKET", width));
            sb.AppendLine(CenterText("*** CREDIT NOTE / GOODS RETURN ***", width));
            sb.AppendLine(CenterText("CAPE TOWN / JHB", width));
            sb.AppendLine("".PadRight(width, '*'));

            sb.AppendLine($"Date:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Doc No:  {creditNoteNo}");
            sb.AppendLine($"Cashier: {cashierName}");
            sb.AppendLine($"Account: {accountName}");
            sb.AppendLine("".PadRight(width, '-'));

            sb.AppendLine("ITEM                 QTY   PRICE    REFUND");
            sb.AppendLine("".PadRight(width, '-'));

            foreach (var item in items)
            {
                string descr = item.Description.Length > 18 ? item.Description[..18] : item.Description;
                double absQty = Math.Abs(item.Qty);
                double absTotal = Math.Abs(item.TotalIncl);
                double unitPrice = absQty != 0 ? absTotal / absQty : 0;
                sb.AppendLine($"{descr,-19} {absQty,3} {unitPrice,7:N2} -{absTotal,8:N2}");
            }

            double absRefund = Math.Abs(refundTotal);
            double vatRate = 0.15;
            double subTotal = Math.Round(absRefund / (1 + vatRate), 2);
            double vat = Math.Round(absRefund - subTotal, 2);

            sb.AppendLine("".PadRight(width, '-'));
            sb.AppendLine($"Refund Subtotal:              -R {subTotal,9:N2}");
            sb.AppendLine($"VAT @ 15%:                    -R {vat,9:N2}");
            sb.AppendLine($"TOTAL REFUND AMOUNT:          -R {absRefund,9:N2}");
            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine("Customer Signature: ______________________");
            sb.AppendLine("Items returned to stock on hand.");
            sb.AppendLine("".PadRight(width, '*'));

            LastReceiptText = sb.ToString();
            LastInvoiceNumber = creditNoteNo;
            return LastReceiptText;
        }

        public static void SaveReceiptToFile(string invoiceNo, string receiptText)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"{invoiceNo}.txt");
                File.WriteAllText(filePath, receiptText);
            }
            catch
            {
                // Non-critical
            }
        }

        private static string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length >= width) return text[..width];
            int leftPad = (width - text.Length) / 2;
            return text.PadLeft(leftPad + text.Length).PadRight(width);
        }
    }
}
