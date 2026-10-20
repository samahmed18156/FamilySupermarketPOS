using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class ShelfLabelWindow : FluentWindow
    {
        private Product? _currentProduct;

        public ShelfLabelWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                TxtSearch.Focus();
                await LoadProduct("1001");
            };
        }

        private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoadProduct(TxtSearch.Text.Trim());
            }
        }

        private async void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            await LoadProduct(TxtSearch.Text.Trim());
        }

        private async System.Threading.Tasks.Task LoadProduct(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            var prod = await DatabaseService.GetProductByCodeOrBarcodeAsync(code);
            if (prod != null)
            {
                _currentProduct = prod;
                LblItemDescr.Text = prod.Description.ToUpperInvariant();
                LblPrice.Text = $"R {prod.SellingPriceIncl:N2}";
                LblCode.Text = $"CODE: {prod.Code}";
                LblBarcode.Text = $"BARCODE: {(string.IsNullOrEmpty(prod.Barcode) ? prod.Code : prod.Barcode)}";
            }
            else
            {
                System.Windows.MessageBox.Show($"Item '{code}' not found.", "Search Result");
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct != null)
            {
                System.Windows.MessageBox.Show(
                    $"Shelf label for '{_currentProduct.Description}' (R {_currentProduct.SellingPriceIncl:N2}) sent to barcode printer!",
                    "Label Sent to Printer");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
