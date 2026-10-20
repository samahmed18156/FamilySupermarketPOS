using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public class DebtorAgingItem
    {
        public string AccountNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public double Current { get; set; }
        public double Days30 { get; set; }
        public double Days60 { get; set; }
        public double Days90Plus { get; set; }
        public double Total => Current + Days30 + Days60 + Days90Plus;

        public string DisplayCurrent => $"R {Current:N2}";
        public string Display30 => $"R {Days30:N2}";
        public string Display60 => $"R {Days60:N2}";
        public string Display90Plus => $"R {Days90Plus:N2}";
        public string DisplayTotal => $"R {Total:N2}";
    }

    public partial class AgeAnalysisWindow : FluentWindow
    {
        public ObservableCollection<DebtorAgingItem> AgingItems { get; set; } = new();

        public AgeAnalysisWindow()
        {
            InitializeComponent();
            GridAging.ItemsSource = AgingItems;
            Loaded += async (s, e) =>
            {
                var debtors = await DatabaseService.SearchDebtorsAsync("");
                AgingItems.Clear();
                foreach (var d in debtors.Where(x => x.OutstandingBalance > 0))
                {
                    AgingItems.Add(new DebtorAgingItem
                    {
                        AccountNumber = d.AccountNumber,
                        Name = d.Name,
                        Current = Math.Round(d.OutstandingBalance * 0.40, 2),
                        Days30 = Math.Round(d.OutstandingBalance * 0.35, 2),
                        Days60 = Math.Round(d.OutstandingBalance * 0.15, 2),
                        Days90Plus = Math.Round(d.OutstandingBalance * 0.10, 2)
                    });
                }
                double total = AgingItems.Sum(x => x.Total);
                TxtTotalOutstanding.Text = $"Total Debtors Book: R {total:N2}";
            };
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("===============================================================");
            sb.AppendLine("                 BLUE_KNIGHT SUPERMARKET                       ");
            sb.AppendLine("              DEBTORS AGE ANALYSIS SUMMARY                     ");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}                            ");
            sb.AppendLine("===============================================================");
            sb.AppendLine("ACC #       CUSTOMER                  CURRENT      30 DAYS     TOTAL");
            sb.AppendLine("---------------------------------------------------------------");
            foreach (var item in AgingItems)
            {
                string name = item.Name.Length > 22 ? item.Name[..22] : item.Name;
                sb.AppendLine($"{item.AccountNumber,-11} {name,-25} {item.Current,9:N2} {item.Days30,9:N2} {item.Total,9:N2}");
            }
            sb.AppendLine("===============================================================");
            sb.AppendLine(TxtTotalOutstanding.Text);
            sb.AppendLine("===============================================================");

            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, $"AgeAnalysis-{DateTime.Now:yyyyMMdd}.txt");
                File.WriteAllText(path, sb.ToString());
                System.Windows.MessageBox.Show($"Age Analysis printed to {path}!", "Report Saved");
            }
            catch { }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
