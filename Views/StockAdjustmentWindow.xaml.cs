using System.Globalization;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class StockAdjustmentWindow : FluentWindow
    {
        private Product? _foundProduct;

        public StockAdjustmentWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => TxtItemCode.Focus();
        }

        private async void TxtItemCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string code = TxtItemCode.Text.Trim();
                if (string.IsNullOrEmpty(code)) return;

                _foundProduct = await DatabaseService.GetProductByCodeOrBarcodeAsync(code);
                if (_foundProduct != null)
                {
                    TxtDescrAndSoh.Text = $"{_foundProduct.Description} (Current SOH: {_foundProduct.Soh:N0})";
                    TxtAdjustmentQty.Focus();
                }
                else
                {
                    System.Windows.MessageBox.Show($"Product '{code}' not found.", "Not Found");
                }
            }
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (_foundProduct == null)
            {
                System.Windows.MessageBox.Show("Please scan or find an item first.", "Notice");
                return;
            }

            if (!double.TryParse(TxtAdjustmentQty.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double qtyChange) || qtyChange == 0)
            {
                System.Windows.MessageBox.Show("Please enter a valid non-zero adjustment quantity (e.g. -1, -5, +10).", "Invalid Quantity");
                return;
            }

            string reason = (CmbReason.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Adjustment";

            bool ok = await DatabaseService.AdjustStockAsync(_foundProduct.Code, qtyChange, reason);
            if (ok)
            {
                double newSoh = _foundProduct.Soh + qtyChange;
                System.Windows.MessageBox.Show(
                    $"Stock adjusted successfully for '{_foundProduct.Description}'!\n\nAdjustment: {(qtyChange > 0 ? "+" : "")}{qtyChange}\nNew SOH: {newSoh:N0}\nReason: {reason}",
                    "Stock Adjusted");

                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to apply stock adjustment.", "Error");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
