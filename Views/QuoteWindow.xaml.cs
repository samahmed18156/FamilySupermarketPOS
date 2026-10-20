using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class QuoteWindow : FluentWindow
    {
        public ObservableCollection<Quote> Quotes { get; set; } = new();
        public Quote? SelectedQuoteToLoad { get; private set; }
        public Quote? LoadedQuote => SelectedQuoteToLoad;
        private List<CartItem>? _cartToSave;

        public QuoteWindow(List<CartItem>? currentCart = null)
        {
            InitializeComponent();
            _cartToSave = currentCart;
            GridQuotes.ItemsSource = Quotes;
            Reload();
        }

        private void Reload()
        {
            Quotes.Clear();
            foreach (var q in DatabaseService.GetQuotes())
            {
                Quotes.Add(q);
            }
            if (Quotes.Count > 0) GridQuotes.SelectedIndex = 0;
        }

        private void BtnSaveCurrentCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cartToSave == null || !_cartToSave.Any())
            {
                System.Windows.MessageBox.Show("There are no items currently in the POS cart to save as a quotation.", "Cart Empty");
                return;
            }

            double total = _cartToSave.Sum(x => x.TotalIncl);
            string qNum = $"QT-{DateTime.Now:yyyyMMdd}-{Quotes.Count + 101}";
            var newQuote = new Quote
            {
                QuoteNumber = qNum,
                CustomerAccount = "CASH",
                CustomerName = "Valued Customer",
                Date = DateTime.Now,
                TotalAmount = total,
                Items = new List<CartItem>(_cartToSave)
            };

            DatabaseService.SaveQuote(newQuote);
            Reload();
            System.Windows.MessageBox.Show($"Quotation {qNum} created successfully for R {total:N2}!", "Quote Saved");
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (GridQuotes.SelectedItem is Quote selected)
            {
                SelectedQuoteToLoad = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a quotation from the list first.", "Select Quote");
            }
        }

        private void BtnPrintQuote_Click(object sender, RoutedEventArgs e)
        {
            if (GridQuotes.SelectedItem is Quote q)
            {
                var sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine("       BLUE_KNIGHT SUPERMARKET          ");
                sb.AppendLine("           FORMAL QUOTATION             ");
                sb.AppendLine("========================================");
                sb.AppendLine($"Quote #:   {q.QuoteNumber}");
                sb.AppendLine($"Date:      {q.DisplayDate}");
                sb.AppendLine($"Customer:  {q.CustomerName} ({q.CustomerAccount})");
                sb.AppendLine("----------------------------------------");
                foreach (var itm in q.Items)
                {
                    sb.AppendLine($"{itm.Description,-20} x{itm.Qty,-2} R {itm.TotalIncl,8:N2}");
                }
                sb.AppendLine("----------------------------------------");
                sb.AppendLine($"TOTAL ESTIMATE: {q.DisplayTotal}");
                sb.AppendLine("========================================");
                sb.AppendLine("Valid for 7 days. Stock subject to availability.");
                sb.AppendLine("========================================");

                try
                {
                    string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                    Directory.CreateDirectory(folder);
                    string filePath = Path.Combine(folder, $"Quote-{q.QuoteNumber}.txt");
                    File.WriteAllText(filePath, sb.ToString());
                }
                catch { }

                System.Windows.MessageBox.Show($"Quotation slip '{q.QuoteNumber}' printed and saved to /Receipts/!", "Quote Printed");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
