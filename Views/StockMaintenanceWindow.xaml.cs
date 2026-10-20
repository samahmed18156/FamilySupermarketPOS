using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class StockMaintenanceWindow : FluentWindow
    {
        public ObservableCollection<Product> StockList { get; set; } = new();
        private bool _isNew = true;

        public StockMaintenanceWindow()
        {
            InitializeComponent();
            GridStock.ItemsSource = StockList;

            Loaded += async (s, e) =>
            {
                await ReloadStock("");
            };
        }

        private async System.Threading.Tasks.Task ReloadStock(string search)
        {
            var items = await DatabaseService.SearchProductsAsync(search);
            StockList.Clear();
            foreach (var item in items)
            {
                StockList.Add(item);
            }
        }

        private async void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            await ReloadStock(TxtSearch.Text.Trim());
        }

        private void GridStock_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridStock.SelectedItem is Product selected)
            {
                _isNew = false;
                FormCode.Text = selected.Code;
                FormCode.IsEnabled = false; // Code cannot be changed once created
                FormBarcode.Text = selected.Barcode;
                FormDescr.Text = selected.Description;
                FormDept.Text = selected.Department;
                FormCost.Text = selected.CostExcl.ToString("0.00", CultureInfo.InvariantCulture);
                FormSelling.Text = selected.SellingPriceIncl.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private void BtnNewItem_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            FormCode.Focus();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _isNew = true;
            FormCode.IsEnabled = true;
            FormCode.Clear();
            FormBarcode.Clear();
            FormDescr.Clear();
            FormDept.Text = "GROCERY";
            FormCost.Text = "0.00";
            FormSelling.Text = "0.00";
            GridStock.SelectedItem = null;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string code = FormCode.Text.Trim();
            string descr = FormDescr.Text.Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(descr))
            {
                System.Windows.MessageBox.Show("Please enter both an Item Code and Description.", "Validation Error");
                return;
            }

            double.TryParse(FormCost.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cost);
            double.TryParse(FormSelling.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double selling);

            var product = new Product
            {
                Code = code,
                Barcode = FormBarcode.Text.Trim(),
                Description = descr,
                Department = FormDept.Text.Trim().ToUpperInvariant(),
                CostExcl = cost,
                SellingPriceIncl = selling,
                Soh = 0
            };

            bool success = await DatabaseService.SaveStockItemAsync(product, _isNew);
            if (success)
            {
                System.Windows.MessageBox.Show($"Product '{descr}' saved successfully!", "Saved");
                await ReloadStock(TxtSearch.Text.Trim());
                ClearForm();
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to save product to database.", "Error");
            }
        }
    }
}
