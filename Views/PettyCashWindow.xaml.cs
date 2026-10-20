using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class PettyCashWindow : FluentWindow
    {
        public ObservableCollection<PettyCashItem> Vouchers { get; set; } = new();

        public PettyCashWindow()
        {
            InitializeComponent();
            GridPettyCash.ItemsSource = Vouchers;
            Reload();
        }

        private void Reload()
        {
            Vouchers.Clear();
            foreach (var item in DatabaseService.GetPettyCashList())
            {
                Vouchers.Add(item);
            }
            double total = Vouchers.Sum(x => x.Amount);
            TxtTotalPettyCash.Text = $"Total Out: R {total:N2}";
        }

        private void BtnIssue_Click(object sender, RoutedEventArgs e)
        {
            string category = (CmbCategory.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "General";
            string descr = TxtDescr.Text.Trim();

            if (string.IsNullOrEmpty(descr))
            {
                System.Windows.MessageBox.Show("Please enter a description for this petty cash payout.", "Validation Error");
                return;
            }

            if (!double.TryParse(TxtAmount.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double amount) || amount <= 0)
            {
                System.Windows.MessageBox.Show("Please enter a valid amount.", "Validation Error");
                return;
            }

            var item = new PettyCashItem
            {
                VoucherNo = $"PC-{new Random().Next(100, 999)}",
                Date = DateTime.Now,
                Category = category,
                Description = descr,
                Amount = amount,
                AuthorizedBy = "ZAMAN MP"
            };

            DatabaseService.SavePettyCash(item);
            Reload();

            TxtDescr.Clear();
            TxtAmount.Clear();

            System.Windows.MessageBox.Show(
                $"Petty Cash Voucher '{item.VoucherNo}' for R {amount:N2} recorded.\n\n[Cash drawer kicked for payout]",
                "Voucher Issued");
        }
    }
}
