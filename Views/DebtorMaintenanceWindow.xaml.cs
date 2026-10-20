using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class DebtorMaintenanceWindow : FluentWindow
    {
        public ObservableCollection<Debtor> Debtors { get; set; } = new();

        public DebtorMaintenanceWindow()
        {
            InitializeComponent();
            GridDebtors.ItemsSource = Debtors;
            Reload();
        }

        private async void Reload()
        {
            Debtors.Clear();
            var list = await DatabaseService.SearchDebtorsAsync("");
            foreach (var d in list) Debtors.Add(d);
            if (Debtors.Count > 0) GridDebtors.SelectedIndex = 0;
        }

        private void GridDebtors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridDebtors.SelectedItem is Debtor d)
            {
                TxtAccNum.Text = d.AccountNumber;
                TxtName.Text = d.Name;
                TxtPhone.Text = d.Telephone;
                TxtLimit.Text = d.CreditLimit.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            TxtAccNum.Clear();
            TxtName.Clear();
            TxtPhone.Clear();
            TxtLimit.Text = "5000.00";
            TxtAccNum.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string acc = TxtAccNum.Text.Trim();
            string name = TxtName.Text.Trim();

            if (string.IsNullOrEmpty(acc) || string.IsNullOrEmpty(name))
            {
                System.Windows.MessageBox.Show("Please enter Account Number and Customer Name.", "Validation Error");
                return;
            }

            double.TryParse(TxtLimit.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double limit);

            DatabaseService.SaveDebtor(new Debtor
            {
                AccountNumber = acc,
                Name = name,
                Telephone = TxtPhone.Text.Trim(),
                CreditLimit = limit,
                OutstandingBalance = 0
            });

            Reload();
            System.Windows.MessageBox.Show($"Customer Account '{name}' ({acc}) saved successfully!", "Saved");
        }
    }
}
