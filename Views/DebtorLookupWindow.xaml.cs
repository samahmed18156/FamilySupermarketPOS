using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class DebtorLookupWindow : FluentWindow
    {
        public ObservableCollection<Debtor> Debtors { get; set; } = new();
        public Debtor? SelectedDebtor { get; private set; }

        public DebtorLookupWindow()
        {
            InitializeComponent();
            GridDebtors.ItemsSource = Debtors;

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
            if (e.Key == Key.Down && Debtors.Count > 0)
            {
                GridDebtors.Focus();
                GridDebtors.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
        }

        private async System.Threading.Tasks.Task PerformSearch(string query)
        {
            var results = await DatabaseService.SearchDebtorsAsync(query);
            Debtors.Clear();
            foreach (var d in results)
            {
                Debtors.Add(d);
            }
            if (Debtors.Count > 0 && GridDebtors.SelectedIndex < 0)
            {
                GridDebtors.SelectedIndex = 0;
            }
        }

        private void GridDebtors_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ConfirmSelection();

        private void GridDebtors_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e) => ConfirmSelection();

        private void ConfirmSelection()
        {
            if (GridDebtors.SelectedItem is Debtor debtor)
            {
                SelectedDebtor = debtor;
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
