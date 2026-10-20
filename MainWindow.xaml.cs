using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
        private bool _isReturnMode = false;
        private DispatcherTimer? _clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            DataGridCart.ItemsSource = Cart;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
            TxtStoreTitle.Text = ConfigService.Current.StoreName;
            TxtCashier.Text = $"Cashier: {ConfigService.Current.CashierName}  |  Station: {ConfigService.Current.StationId}";

            // Live digital clock timer
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) =>
            {
                TxtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            _clockTimer.Start();
            TxtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");

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
            else if (e.Key == Key.F5)
            {
                ApplyDiscount();
                e.Handled = true;
            }
            else if (e.Key == Key.F6)
            {
                ApplyPriceOverride();
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                ToggleReturnMode();
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

        private void ToggleReturnMode()
        {
            _isReturnMode = !_isReturnMode;
            BadgeReturnMode.Visibility = _isReturnMode ? Visibility.Visible : Visibility.Collapsed;
            TxtReturnBtnLabel.Text = _isReturnMode ? "EXIT RETURN" : "Return";
            BtnToggleReturn.Background = _isReturnMode 
                ? new SolidColorBrush(Color.FromRgb(185, 28, 28)) 
                : new SolidColorBrush(Color.FromRgb(56, 16, 20));
        }

        private void BtnToggleReturn_Click(object sender, RoutedEventArgs e) => ToggleReturnMode();

        private void BtnDiscount_Click(object sender, RoutedEventArgs e) => ApplyDiscount();

        private void ApplyDiscount()
        {
            if (!Cart.Any())
            {
                System.Windows.MessageBox.Show("Please scan at least one item before applying a discount.", "Cart Empty");
                return;
            }

            var selected = DataGridCart.SelectedItem as CartItem ?? Cart.Last();
            var discWin = new DiscountWindow(selected.TotalIncl) { Owner = this };
            if (discWin.ShowDialog() == true && discWin.DiscountPercent > 0)
            {
                double pct = discWin.DiscountPercent;
                selected.Discount = pct;
                selected.TotalIncl = Math.Round(selected.Qty * selected.PriceExcl * (1 + VatRate) * (1 - pct / 100.0), 2);
                int idx = Cart.IndexOf(selected);
                Cart[idx] = selected;
                UpdateTotals();
                System.Windows.MessageBox.Show($"Discount of {pct}% applied to item '{selected.Description}'.", "Discount Applied");
            }
        }

        private void BtnPriceOverride_Click(object sender, RoutedEventArgs e) => ApplyPriceOverride();

        private void ApplyPriceOverride()
        {
            if (!Cart.Any())
            {
                System.Windows.MessageBox.Show("Please scan at least one item before overriding price.", "Cart Empty");
                return;
            }

            var selected = DataGridCart.SelectedItem as CartItem ?? Cart.Last();
            double currentUnitPrice = selected.Qty != 0 ? Math.Abs(selected.TotalIncl / selected.Qty) : selected.PriceExcl;
            var priceWin = new PriceOverrideWindow(selected.Description, currentUnitPrice) { Owner = this };
            if (priceWin.ShowDialog() == true && priceWin.NewPrice >= 0)
            {
                double newPrice = priceWin.NewPrice;
                selected.TotalIncl = Math.Round(selected.Qty * newPrice, 2);
                selected.PriceExcl = Math.Round(newPrice / (1 + VatRate), 2);
                int idx = Cart.IndexOf(selected);
                Cart[idx] = selected;
                UpdateTotals();
                System.Windows.MessageBox.Show($"Price updated to R {newPrice:N2} for '{selected.Description}'.", "Price Overridden");
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

                if (_isReturnMode)
                {
                    qty = -Math.Abs(qty);
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
                existing.PriceExcl = Math.Round(product.SellingPriceExcl, 2);
                existing.IsReturn = existing.Qty < 0;

                int idx = Cart.IndexOf(existing);
                Cart[idx] = existing;
            }
            else
            {
                double totalIncl = Math.Round(product.SellingPriceIncl * qty, 2);
                double priceExcl = Math.Round(product.SellingPriceExcl, 2);

                Cart.Add(new CartItem
                {
                    Code = product.Code,
                    Description = product.Description,
                    Qty = qty,
                    PriceExcl = priceExcl,
                    TotalIncl = totalIncl,
                    IsReturn = qty < 0
                });
            }

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            EmptyCartWatermark.Visibility = Cart.Any() ? Visibility.Collapsed : Visibility.Visible;

            double grandTotal = Cart.Sum(x => x.TotalIncl);
            double subTotal = Math.Round(grandTotal / (1 + VatRate), 2);
            double vat = Math.Round(grandTotal - subTotal, 2);

            TxtSubtotal.Text = $"Sub Total (Excl): R {subTotal:N2}";
            TxtVat.Text = $"VAT (15%):           R {vat:N2}";
            TxtTotal.Text = $"R {grandTotal:N2}";
            TxtItemsCount.Text = $"Items: {Cart.Sum(x => Math.Abs(x.Qty))}";
        }

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is CartItem item)
            {
                Cart.Remove(item);
                UpdateTotals();
                TxtBarcode.Focus();
            }
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

            // If grand total is negative, it's a Return / Credit Note refund
            if (grandTotal < 0)
            {
                string creditNo = HardwareService.GenerateNextCreditNoteNumber();
                string creditSlip = HardwareService.GenerateCreditNoteSlip(creditNo, Cart.ToList(), grandTotal, ConfigService.Current.CashierName, accName);
                HardwareService.SaveReceiptToFile(creditNo, creditSlip);

                // Return items back to stock
                await DatabaseService.RestituteStockAsync(creditNo, Cart.ToList());

                System.Windows.MessageBox.Show(
                    $"CREDIT NOTE / RETURN PROCESSED!\n\nCredit Note: {creditNo}\nTotal Refund: R {Math.Abs(grandTotal):N2}\nItems restocked into inventory.\n\n[Credit Note Slip Saved to /Receipts/ folder]\n[Cash Drawer Kicked Open for Refund]",
                    "Credit Note Completed");

                Cart.Clear();
                _isReturnMode = false;
                BadgeReturnMode.Visibility = Visibility.Collapsed;
                TxtReturnBtnLabel.Text = "Return";
                BtnToggleReturn.Background = new SolidColorBrush(Color.FromRgb(56, 16, 20));
                UpdateTotals();
                TxtBarcode.Focus();
                return;
            }

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
                double qty = _isReturnMode ? -1 : 1;
                AddItemToCart(searchWin.SelectedProduct, qty);
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
                TxtCreditLimit.Text = $"Limit: {d.DisplayLimit}";
                TxtOsBal.Text = $"Balance: {d.DisplayBalance}";
            }
            TxtBarcode.Focus();
        }

        private void BtnCompleteSale_Click(object sender, RoutedEventArgs e) => CompleteSale();

        private void BtnSearch_Click(object sender, RoutedEventArgs e) => OpenSearch();

        private void BtnAccountLookup_Click(object sender, RoutedEventArgs e) => OpenDebtorLookup();

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MenuSwitchUser_Click(object sender, RoutedEventArgs e)
        {
            var loginWin = new LoginWindow { Owner = this };
            if (loginWin.ShowDialog() == true && loginWin.LoggedInUser != null)
            {
                ConfigService.Current.CashierName = loginWin.LoggedInUser.FullName;
                TxtCashier.Text = $"Cashier: {ConfigService.Current.CashierName}  |  Station: {ConfigService.Current.StationId}";
                System.Windows.MessageBox.Show($"Switched to user {loginWin.LoggedInUser.FullName} ({loginWin.LoggedInUser.Role}).", "User Switched");
            }
        }

        private void MenuLock_Click(object sender, RoutedEventArgs e)
        {
            var loginWin = new LoginWindow { Owner = this };
            loginWin.ShowDialog();
        }

        private void MenuStockMaint_Click(object sender, RoutedEventArgs e)
        {
            var maintWin = new StockMaintenanceWindow { Owner = this };
            maintWin.ShowDialog();
        }

        private void MenuDeptMaint_Click(object sender, RoutedEventArgs e)
        {
            var deptWin = new DepartmentMaintenanceWindow { Owner = this };
            deptWin.ShowDialog();
        }

        private void MenuStockAdj_Click(object sender, RoutedEventArgs e)
        {
            var adjWin = new StockAdjustmentWindow { Owner = this };
            adjWin.ShowDialog();
        }

        private void MenuStockTake_Click(object sender, RoutedEventArgs e)
        {
            var stWin = new StockTakeWindow { Owner = this };
            stWin.ShowDialog();
        }

        private void MenuShelfLabels_Click(object sender, RoutedEventArgs e)
        {
            var labelWin = new ShelfLabelWindow { Owner = this };
            labelWin.ShowDialog();
        }

        private void MenuGrn_Click(object sender, RoutedEventArgs e)
        {
            var grnWin = new GrnWindow { Owner = this };
            grnWin.ShowDialog();
        }

        private void MenuCreditorMaint_Click(object sender, RoutedEventArgs e)
        {
            var credWin = new CreditorMaintenanceWindow { Owner = this };
            credWin.ShowDialog();
        }

        private void MenuDebtorMaint_Click(object sender, RoutedEventArgs e)
        {
            var debtorMaintWin = new DebtorMaintenanceWindow { Owner = this };
            debtorMaintWin.ShowDialog();
        }

        private void MenuLaybye_Click(object sender, RoutedEventArgs e)
        {
            var laybyeWin = new LaybyeWindow { Owner = this };
            laybyeWin.ShowDialog();
        }

        private void MenuQuotes_Click(object sender, RoutedEventArgs e)
        {
            var quoteWin = new QuoteWindow(Cart.ToList()) { Owner = this };
            if (quoteWin.ShowDialog() == true && quoteWin.LoadedQuote != null)
            {
                Cart.Clear();
                foreach (var item in quoteWin.LoadedQuote.Items)
                {
                    Cart.Add(item);
                }
                UpdateTotals();
                System.Windows.MessageBox.Show($"Quotation {quoteWin.LoadedQuote.QuoteNumber} loaded into current POS cart.", "Quote Loaded");
            }
        }

        private void MenuAgeAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var ageWin = new AgeAnalysisWindow { Owner = this };
            ageWin.ShowDialog();
        }

        private void MenuPettyCash_Click(object sender, RoutedEventArgs e)
        {
            var pcWin = new PettyCashWindow { Owner = this };
            pcWin.ShowDialog();
        }

        private void MenuReports_Click(object sender, RoutedEventArgs e)
        {
            var repWin = new ReportsWindow { Owner = this };
            repWin.ShowDialog();
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

        private void MenuMonthEnd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Month End Process:\n1. Aging of Debtor Balances (Current -> 30 -> 60 -> 90 -> 120+ days)\n2. Generate Customer Monthly Statements\n3. Close general ledger calendar period.\n\nBackup database before proceeding.",
                "Month End Process");
        }

        private void MenuUserMaint_Click(object sender, RoutedEventArgs e)
        {
            var userWin = new UserMaintenanceWindow { Owner = this };
            userWin.ShowDialog();
        }

        private void MenuCompanyDetails_Click(object sender, RoutedEventArgs e)
        {
            var compWin = new CompanyDetailsWindow { Owner = this };
            compWin.ShowDialog();
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
                "Blue_Knight POS - Enterprise Retail Terminal Suite\nVersion 1.0 (Windows 10/11 64-bit)\nBuilt with .NET 8 & WPF-UI Fluent Design\n\nOptimized for Supermarket & Wholesale Store Operations\nFull Parity with MiPOS Java Legacy System",
                "About Blue_Knight POS");
        }
    }
}
