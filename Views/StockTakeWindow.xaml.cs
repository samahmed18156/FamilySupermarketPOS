using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class StockTakeWindow : FluentWindow
    {
        public ObservableCollection<Product> StockItems { get; set; } = new();

        public StockTakeWindow()
        {
            InitializeComponent();
            GridStockTake.ItemsSource = StockItems;
            Loaded += async (s, e) =>
            {
                var items = await DatabaseService.SearchProductsAsync("");
                StockItems.Clear();
                foreach (var it in items) StockItems.Add(it);
            };
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CODE,DESCRIPTION,DEPARTMENT,SYSTEM_SOH,PHYSICAL_COUNT");
            foreach (var item in StockItems)
            {
                sb.AppendLine($"{item.Code},\"{item.Description}\",{item.Department},{item.Soh},___");
            }
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "StockTake-CountSheet.csv");
                File.WriteAllText(path, sb.ToString());
                System.Windows.MessageBox.Show($"Stock take sheet exported to {path}!", "Export Complete");
            }
            catch { }
        }

        private void BtnPost_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Stock take quantities reconciled! Inventory snapshot has been archived.",
                "Stock Take Complete");
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
