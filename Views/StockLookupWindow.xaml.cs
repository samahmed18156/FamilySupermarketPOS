using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class StockLookupWindow : FluentWindow
    {
        public ObservableCollection<Product> Products { get; set; } = new();
        public Product? SelectedProduct { get; private set; }

        public StockLookupWindow()
        {
            InitializeComponent();
            GridProducts.ItemsSource = Products;

            Loaded += async (s, e) =>
            {
                TxtSearch.Focus();
                await PerformSearch("");
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                    e.Handled = true;
                }
            };
        }

        private async void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            await PerformSearch(TxtSearch.Text.Trim());
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && Products.Count > 0)
            {
                GridProducts.Focus();
                GridProducts.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch(TxtSearch.Text.Trim());
        }

        private async System.Threading.Tasks.Task PerformSearch(string query)
        {
            var results = await DatabaseService.SearchProductsAsync(query);
            Products.Clear();
            foreach (var p in results)
            {
                Products.Add(p);
            }
            if (Products.Count > 0 && GridProducts.SelectedIndex < 0)
            {
                GridProducts.SelectedIndex = 0;
            }
        }

        private void GridProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void GridProducts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmSelection()
        {
            if (GridProducts.SelectedItem is Product prod)
            {
                SelectedProduct = prod;
                DialogResult = true;
                Close();
            }
        }
    }
}
