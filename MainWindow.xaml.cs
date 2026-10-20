using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Services;
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
        private const double VatRate = 0.15; // 15% South African VAT
        public ObservableCollection<CartItem> Cart { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataGridCart.ItemsSource = Cart;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd MMMM yyyy");

            Loaded += async (s, e) =>
            {
                TxtBarcode.Focus();
                bool dbConnected = await DatabaseService.TestConnectionAsync();
                if (!dbConnected)
                {
                    System.Windows.MessageBox.Show(
                        "Note: Could not connect to local MySQL database.\nFalling back to demo item codes (1001, 1002, 1003).",
                        "Database Notice");
                }
            };

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

        private async void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string code = TxtBarcode.Text.Trim();
                if (string.IsNullOrEmpty(code)) return;

                if (!double.TryParse(TxtQty.Text.Trim(), out double qty) || qty <= 0)
                {
                    qty = 1;
                }

                // 1. Try fetching from MySQL database
                var dbItem = await DatabaseService.GetProductByCodeOrBarcodeAsync(code);

                string descr = "";
                double priceIncl = 0;

                if (dbItem != null)
                {
                    descr = dbItem.Description;
                    priceIncl = dbItem.SellingPriceIncl;
                }
                else
                {
                    // Fallback local demo items if MySQL is offline
                    if (code == "1001") { descr = "Full Cream Milk 2L"; priceIncl = 32.99; }
                    else if (code == "1002") { descr = "White Bread 700g"; priceIncl = 18.50; }
                    else if (code == "1003") { descr = "Sunflower Oil 2L"; priceIncl = 79.99; }
                    else
                    {
                        System.Windows.MessageBox.Show($"Item code or barcode '{code}' not found.", "Not Found");
                        TxtBarcode.SelectAll();
                        return;
                    }
                }

                double totalIncl = Math.Round(priceIncl * qty, 2);
                double priceExcl = Math.Round(priceIncl / (1 + VatRate), 2);

                Cart.Add(new CartItem
                {
                    Code = code,
                    Description = descr,
                    Qty = qty,
                    PriceExcl = priceExcl,
                    TotalIncl = totalIncl
                });

                UpdateTotals();

                TxtBarcode.Clear();
                TxtQty.Text = "1";
                TxtBarcode.Focus();
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
            TxtItemsCount.Text = $"Num Items: {Cart.Sum(x => x.Qty)}";
        }

        private void CompleteSale()
        {
            if (!Cart.Any())
            {
                System.Windows.MessageBox.Show("Please scan at least one item first.", "Empty Sale");
                return;
            }

            double grandTotal = Cart.Sum(x => x.TotalIncl);
            System.Windows.MessageBox.Show($"Sale Completed!\nTotal Tendered: R {grandTotal:N2}\n\n[Triggering Cash Drawer & ESC/POS Printer]", "Sale Finished");

            Cart.Clear();
            UpdateTotals();
            TxtBarcode.Focus();
        }

        private void BtnCompleteSale_Click(object sender, RoutedEventArgs e) => CompleteSale();

        private void BtnSearch_Click(object sender, RoutedEventArgs e) => OpenSearch();

        private void OpenSearch()
        {
            System.Windows.MessageBox.Show("F3 Stock Lookup dialog will open here.", "Stock Lookup");
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MenuSwitchUser_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Switch Cashier login dialog.", "Switch User");
        }

        private void MenuStockMaint_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Stock Maintenance window will open here.", "Stock Maintenance");
        }
    }
}