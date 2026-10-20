using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class LaybyeWindow : FluentWindow
    {
        public ObservableCollection<Laybye> Laybyes { get; set; } = new();

        public LaybyeWindow()
        {
            InitializeComponent();
            GridLaybyes.ItemsSource = Laybyes;
            Reload();
        }

        private void Reload()
        {
            Laybyes.Clear();
            foreach (var lb in DatabaseService.GetLaybyes())
            {
                Laybyes.Add(lb);
            }
            if (Laybyes.Count > 0) GridLaybyes.SelectedIndex = 0;
        }

        private void GridLaybyes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridLaybyes.SelectedItem is Laybye selected)
            {
                TxtPaymentAmount.Text = selected.Balance.ToString("0.00", CultureInfo.InvariantCulture);
                BtnRelease.IsEnabled = selected.Balance <= 0;
            }
        }

        private void BtnPayInstallment_Click(object sender, RoutedEventArgs e)
        {
            if (GridLaybyes.SelectedItem is Laybye selected)
            {
                if (double.TryParse(TxtPaymentAmount.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double amount) && amount > 0)
                {
                    selected.AmountPaid += amount;
                    DatabaseService.SaveLaybye(selected);
                    Reload();
                    System.Windows.MessageBox.Show(
                        $"Payment of R {amount:N2} captured for {selected.CustomerName}.\nRemaining Balance: {selected.DisplayBalance}",
                        "Laybye Payment Received");
                }
            }
        }

        private void BtnRelease_Click(object sender, RoutedEventArgs e)
        {
            if (GridLaybyes.SelectedItem is Laybye selected)
            {
                if (selected.Balance > 0)
                {
                    System.Windows.MessageBox.Show("Cannot release goods: Laybye balance is not fully paid yet.", "Balance Outstanding");
                    return;
                }

                selected.Status = "Completed";
                DatabaseService.SaveLaybye(selected);
                System.Windows.MessageBox.Show($"Goods released to customer {selected.CustomerName}! Laybye finalized.", "Goods Released");
                Reload();
            }
        }

        private void BtnNewLaybye_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("To create a new Laybye, ring up the items on the main cash screen, then press F12 and select 'Account/Laybye'!", "Create Laybye");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
