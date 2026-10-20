using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using FamilySupermarketPOS.Views;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS
{
    public partial class MainWindow : FluentWindow
    {
        private const double VatRate = 0.15; // 15% South African VAT
        public ObservableCollection<CartItem> Cart { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataGridCart.ItemsSource = Cart;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
            TxtStoreTitle.Text = ConfigService.Current.StoreName;
            TxtCashier.Text = $"Cashier: {ConfigService.Current.CashierName}  |  Station: {ConfigService.Current.StationId}  |  Mode: Retail POS";

            Loaded += async (s, e) =>
            {
                TxtBarcode.Focus();
                bool dbConnected = await DatabaseService.TestConnectionAsync();
                if (!dbConnected)
                {
                    System.Windows.MessageBox.Show(
                        "Notice: Could not connect to local MySQL database.\nOffline mode active: Using demo stock catalog.\n\nTip: Go to Utility -> MySQL Database Configuration to set your MySQL password.",
                        "Blue_Knight POS");
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
            else if (e.Key == Key.F10)
            {
                OpenReprint();
                e.Handled = true;
            }
            else if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenDebtorLookup();
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

                var product = await DatabaseService.GetProductByCodeOrBarcodeAsync(code);
                if (product != null)
                {
                    AddItemToCart(product, qty);
                    TxtBarcode.Clear();
                    TxtQty.Text = "1";
                    TxtBarcode.Focus();
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Product with code/barcode '{code}' was not found in database.\nPress F3 to search by name.",
                        "Product Not Found");
                    TxtBarcode.SelectAll();
                }
            }
        }

        private void AddItemToCart(Product product, double qty)
        {
            var existing = Cart.FirstOrDefault(x => x.Code.Equals(product.Code, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Qty += qty;
                existing.TotalIncl = Math.Round(existing.Qty * product.SellingPriceIncl, 2);
                existing.PriceExcl = Math.Round(existing.TotalIncl / (1 + VatRate), 2);
                
                int idx = Cart.IndexOf(existing);
                Cart[idx] = existing;
            }
            else
            {
                double totalIncl = Math.Round(product.SellingPriceIncl * qty, 2);
                double priceExcl = Math.Round(totalIncl / (1 + VatRate), 2);

                Cart.Add(new CartItem
                {
                    Code = product.Code,
                    Description = product.Description,
                    Qty = qty,
                    PriceExcl = priceExcl,
                    TotalIncl = totalIncl
                });
            }

            UpdateTotals();
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

        private async void CompleteSale()
        {
            if (!Cart.Any())
            {
                System.Windows.MessageBox.Show("Cart is empty. Please scan or search at least one item first.", "Empty Sale");
                return;
            }

            double grandTotal = Cart.Sum(x => x.TotalIncl);
            string accName = $"{TxtAccountNum.Text.Trim()} ({TxtAccountName.Text.Trim()})";

            var payWin = new PaymentWindow(grandTotal) { Owner = this };
            if (payWin.ShowDialog() == true && payWin.Result.Success)
            {
                string invoiceNo = HardwareService.GenerateNextInvoiceNumber();
                string slip = HardwareService.GenerateReceiptSlip(
                    invoiceNo,
                    Cart.ToList(),
                    grandTotal,
                    payWin.Result.AmountTendered,
                    payWin.Result.Change,
                    payWin.Result.PaymentMethod,
                    ConfigService.Current.CashierName,
                    accName);

                HardwareService.SaveReceiptToFile(invoiceNo, slip);
                await DatabaseService.RecordSaleTransactionAsync(invoiceNo, Cart.ToList(), payWin.Result.PaymentMethod);

                string changeInfo = payWin.Result.PaymentMethod == "Cash" 
                    ? $"\nChange: R {payWin.Result.Change:N2}" 
                    : "";

                System.Windows.MessageBox.Show(
                    $"Sale Completed Successfully!\n\nInvoice: {invoiceNo}\nMethod:  {payWin.Result.PaymentMethod}\nTender:  R {payWin.Result.AmountTendered:N2}{changeInfo}\n\n[Receipt slip saved to /Receipts/ folder]\n[Cash Drawer Kicked Open]",
                    "Transaction Finished");

                Cart.Clear();
                UpdateTotals();
                TxtBarcode.Focus();
            }
        }

        private void OpenSearch()
        {
            var searchWin = new StockLookupWindow { Owner = this };
            if (searchWin.ShowDialog() == true && searchWin.SelectedProduct != null)
            {
                AddItemToCart(searchWin.SelectedProduct, 1);
            }
            TxtBarcode.Focus();
        }

        private void OpenReprint()
        {
            var reprintWin = new ReceiptPreviewWindow { Owner = this };
            reprintWin.ShowDialog();
        }

        private void OpenDebtorLookup()
        {
            var debtorWin = new DebtorLookupWindow { Owner = this };
            if (debtorWin.ShowDialog() == true && debtorWin.SelectedDebtor != null)
            {
                var d = debtorWin.SelectedDebtor;
                TxtAccountNum.Text = d.AccountNumber;
                TxtAccountName.Text = d.Name;
                TxtCreditLimit.Text = $"Credit Limit: {d.DisplayLimit}";
                TxtOsBal.Text = $"OS Balance: {d.DisplayBalance}";
            }
            TxtBarcode.Focus();
        }

        private void BtnCompleteSale_Click(object sender, RoutedEventArgs e) => CompleteSale();

        private void BtnSearch_Click(object sender, RoutedEventArgs e) => OpenSearch();

        private void BtnAccountLookup_Click(object sender, RoutedEventArgs e) => OpenDebtorLookup();

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MenuSwitchUser_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Switch Cashier login dialog.", "Switch Cashier");
        }

        private void MenuStockMaint_Click(object sender, RoutedEventArgs e)
        {
            var maintWin = new StockMaintenanceWindow { Owner = this };
            maintWin.ShowDialog();
        }

        private void MenuDebtors_Click(object sender, RoutedEventArgs e) => OpenDebtorLookup();

        private void MenuCashUp_Click(object sender, RoutedEventArgs e)
        {
            var cashUpWin = new CashUpWindow { Owner = this };
            cashUpWin.ShowDialog();
        }

        private void MenuDayEnd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Day End Process will:\n1. Lock all current day transactions\n2. Consolidate cash, card, and debtor totals\n3. Roll stock on hand forward for tomorrow.\n\nPlease complete Cash Up first.",
                "Day End Process");
        }

        private void MenuDatabaseConfig_Click(object sender, RoutedEventArgs e)
        {
            var dbWin = new DatabaseConfigWindow { Owner = this };
            dbWin.ShowDialog();
        }

        private void MenuReprint_Click(object sender, RoutedEventArgs e) => OpenReprint();

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Blue_Knight POS - Retail Terminal Edition\nVersion 1.0 (Windows 10/11 64-bit)\nBuilt with .NET 8 & WPF-UI Fluent Design\n\nOptimized for Family Supermarket / Retail Store Operations",
                "About Blue_Knight POS");
        }
    }
}
