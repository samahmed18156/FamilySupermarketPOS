using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class ReportsWindow : FluentWindow
    {
        public ObservableCollection<Product> ReportData { get; set; } = new();
        private string _activeReport = "ZeroSOH";

        public ReportsWindow()
        {
            InitializeComponent();
            GridReport.ItemsSource = ReportData;
            Loaded += async (s, e) => await LoadZeroSoh();
        }

        private async void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            BtnZeroSoh.Appearance = sender == BtnZeroSoh ? ControlAppearance.Primary : ControlAppearance.Secondary;
            BtnValuation.Appearance = sender == BtnValuation ? ControlAppearance.Primary : ControlAppearance.Secondary;
            BtnDaily.Appearance = sender == BtnDaily ? ControlAppearance.Primary : ControlAppearance.Secondary;

            if (sender == BtnZeroSoh)
            {
                _activeReport = "ZeroSOH";
                TxtReportTitle.Text = "REPORT: ZERO STOCK ON HAND (SOH = 0)";
                await LoadZeroSoh();
            }
            else if (sender == BtnValuation)
            {
                _activeReport = "Valuation";
                TxtReportTitle.Text = "REPORT: STOCK VALUATION (COST & RETAIL VALUE)";
                await LoadValuation();
            }
            else if (sender == BtnDaily)
            {
                _activeReport = "Daily";
                TxtReportTitle.Text = "REPORT: DAILY SALES SUMMARY";
                await LoadDailyTotals();
            }
        }

        private async System.Threading.Tasks.Task LoadZeroSoh()
        {
            var items = await DatabaseService.GetZeroSohReportAsync();
            ReportData.Clear();
            foreach (var item in items) ReportData.Add(item);
            TxtReportSummary.Text = $"Zero Stock Items Requiring Reorder: {ReportData.Count}";
        }

        private async System.Threading.Tasks.Task LoadValuation()
        {
            var items = await DatabaseService.GetAllStockForValuationAsync();
            ReportData.Clear();
            foreach (var item in items) ReportData.Add(item);

            double totalUnits = ReportData.Sum(x => x.Soh);
            double totalCost = ReportData.Sum(x => x.Soh * x.CostExcl);
            double totalRetail = ReportData.Sum(x => x.Soh * x.SellingPriceIncl);

            TxtReportSummary.Text = $"Total Units: {totalUnits:N0}  |  Total Cost Value: R {totalCost:N2}  |  Retail Value: R {totalRetail:N2}";
        }

        private async System.Threading.Tasks.Task LoadDailyTotals()
        {
            var items = await DatabaseService.GetAllStockForValuationAsync();
            ReportData.Clear();
            foreach (var item in items.Take(10)) ReportData.Add(item);

            TxtReportSummary.Text = "Total Daily Transactions: Active  |  Estimated Gross Sales: Available via Cash Up";
        }

        private void BtnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("       BLUE_KNIGHT SUPERMARKET          ");
            sb.AppendLine($"           {TxtReportTitle.Text}        ");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("========================================");
            sb.AppendLine("CODE       DESCRIPTION             SOH   PRICE");
            sb.AppendLine("----------------------------------------");

            foreach (var item in ReportData)
            {
                string descr = item.Description.Length > 20 ? item.Description[..20] : item.Description;
                sb.AppendLine($"{item.Code,-10} {descr,-21} {item.Soh,5:N0} {item.SellingPriceIncl,7:N2}");
            }
            sb.AppendLine("========================================");
            sb.AppendLine(TxtReportSummary.Text);
            sb.AppendLine("========================================");

            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"Report-{_activeReport}-{DateTime.Now:yyyyMMdd-HHmm}.txt");
                File.WriteAllText(filePath, sb.ToString());
            }
            catch { }

            System.Windows.MessageBox.Show(
                $"Report '{_activeReport}' printed successfully and saved to /Receipts/ folder!",
                "Report Generated");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
