using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS
{
    public class CartItem
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public double Qty { get; set; }
        public double PriceExcl { get; set; }
        public double TotalIncl { get; set; }

        public string PriceExclDisplay => $"R {PriceExcl:N2}";
        public string TotalInclDisplay => $"R {TotalIncl:N2}";
    }

    public partial class MainWindow : FluentWindow
    {
        // 15% South African VAT
        private const double VatRate = 0.15;

        public ObservableCollection<CartItem> Cart { get; set; } = new();

        // Sample stock catalog
        private readonly Dictionary<string, (string Descr, double PriceIncl)> _stock = new()
        {
            { "1001", ("Full Cream Milk 2L", 32.99) },
            { "1002", ("White Bread 700g", 18.50) },
            { "1003", ("Sunflower Cooking Oil 2L", 79.99) },
            { "1004", ("White Sugar 2.5kg", 54.00) },
            { "1005", ("Coca Cola 2L", 26.99) },
            { "1006", ("Large Eggs 30-Pack", 89.99) }
        };

        public MainWindow()
        {
            InitializeComponent();
            DataGridCart.ItemsSource = Cart;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
            Loaded += (s, e) => TxtBarcode.Focus();

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                CompleteSale();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                OpenSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && DataGridCart.SelectedItem is CartItem selected)
            {
                Cart.Remove(selected);
                UpdateTotals();
                e.Handled = true;
            }
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string code = TxtBarcode.Text.Trim();
                if (string.IsNullOrEmpty(code)) return;

                if (!double.TryParse(TxtQty.Text.Trim(), out double qty) || qty <= 0)
                {
                    qty = 1;
                }

                if (_stock.TryGetValue(code, out var item))
                {
                    double totalIncl = Math.Round(item.PriceIncl * qty, 2);
                    double priceExcl = Math.Round(item.PriceIncl / (1 + VatRate), 2);

                    Cart.Add(new CartItem
                    {
                        Code = code,
                        Description = item.Descr,
                        Qty = qty,
                        PriceExcl = priceExcl,
                        TotalIncl = totalIncl
                    });

                    UpdateTotals();

                    TxtBarcode.Clear();
                    TxtQty.Text = "1";
                    TxtBarcode.Focus();
                }
                else
                {
                    System.Windows.MessageBox.Show($"Product with code '{code}' not found.\nTry: 1001, 1002, 1003, or 1004.");
                    TxtBarcode.SelectAll();
                }
            }
        }

        private void UpdateTotals()
        {
            double grandTotal = Cart.Sum(x => x.TotalIncl);
            double subTotal = Math.Round(grandTotal / (1 + VatRate), 2);
            double vat = Math.Round(grandTotal - subTotal, 2);

            TxtSubtotal.Text = $"Sub Total: R {subTotal:N2}";
            TxtVat.Text = $"VAT (15%): R {vat:N2}";
            TxtTotal.Text = $"R {grandTotal:N2}";
            TxtItemsCount.Text = $"Items: {Cart.Sum(x => x.Qty)}";
        }

        private void CompleteSale()
        {
            if (!Cart.Any())
            {
                System.Windows.MessageBox.Show("Please scan at least one item first.");
                return;
            }

            double grandTotal = Cart.Sum(x => x.TotalIncl);
            System.Windows.MessageBox.Show($"Sale Completed!\nTotal Tendered: R {grandTotal:N2}\n\n[Triggering Cash Drawer & Receipt Printer]");

            Cart.Clear();
            UpdateTotals();
            TxtBarcode.Focus();
        }

        private void BtnCompleteSale_Click(object sender, RoutedEventArgs e) => CompleteSale();

        private void BtnSearch_Click(object sender, RoutedEventArgs e) => OpenSearch();

        private void OpenSearch()
        {
            System.Windows.MessageBox.Show("F3 Stock Lookup dialog will open here.");
        }
    }
}