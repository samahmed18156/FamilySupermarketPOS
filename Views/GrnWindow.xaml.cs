using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class GrnWindow : FluentWindow
    {
        public ObservableCollection<CartItem> GrnItems { get; set; } = new();
        private Product? _currentItem;

        public GrnWindow()
        {
            InitializeComponent();
            GridGrnItems.ItemsSource = GrnItems;
            TxtGrnNo.Text = $"GRN-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";

            Loaded += async (s, e) =>
            {
                var creditors = await DatabaseService.GetCreditorsAsync();
                CmbSupplier.ItemsSource = creditors;
                if (creditors.Count > 0) CmbSupplier.SelectedIndex = 0;
                TxtItemCode.Focus();
            };
        }

        private async void TxtItemCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string code = TxtItemCode.Text.Trim();
                if (string.IsNullOrEmpty(code)) return;

                _currentItem = await DatabaseService.GetProductByCodeOrBarcodeAsync(code);
                if (_currentItem != null)
                {
                    TxtItemDescr.Text = _currentItem.Description;
                    TxtItemCost.Text = _currentItem.CostExcl.ToString("0.00", CultureInfo.InvariantCulture);
                    TxtItemQty.Focus();
                    TxtItemQty.SelectAll();
                }
                else
                {
                    System.Windows.MessageBox.Show($"Product code '{code}' not found.", "Item Not Found");
                }
            }
        }

        private void BtnAddLine_Click(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null)
            {
                System.Windows.MessageBox.Show("Please scan or enter an item code first.", "Notice");
                return;
            }

            double.TryParse(TxtItemQty.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double qty);
            double.TryParse(TxtItemCost.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cost);

            if (qty <= 0) qty = 1;
            double lineTotal = Math.Round(qty * cost, 2);

            GrnItems.Add(new CartItem
            {
                Code = _currentItem.Code,
                Description = _currentItem.Description,
                Qty = qty,
                PriceExcl = cost,
                TotalIncl = lineTotal
            });

            UpdateTotals();

            _currentItem = null;
            TxtItemCode.Clear();
            TxtItemDescr.Clear();
            TxtItemCost.Clear();
            TxtItemQty.Text = "1";
            TxtItemCode.Focus();
        }

        private void UpdateTotals()
        {
            double totalCost = GrnItems.Sum(x => x.TotalIncl);
            TxtTotalCostDisplay.Text = $"Total GRN Cost: R {totalCost:N2}";
        }

        private async void BtnProcessGrn_Click(object sender, RoutedEventArgs e)
        {
            if (!GrnItems.Any())
            {
                System.Windows.MessageBox.Show("Please add at least one received item.", "Empty GRN");
                return;
            }

            string grnNo = TxtGrnNo.Text;
            string supplier = CmbSupplier.SelectedValue?.ToString() ?? "SUPP";
            string invNo = TxtSupplierInv.Text.Trim();

            bool ok = await DatabaseService.ProcessGrnAsync(grnNo, supplier, invNo, GrnItems.ToList());
            if (ok)
            {
                System.Windows.MessageBox.Show(
                    $"GRN '{grnNo}' processed successfully!\n\nStock on hand has been updated for {GrnItems.Count} items.",
                    "GRN Processed");
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to process GRN.", "Error");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
