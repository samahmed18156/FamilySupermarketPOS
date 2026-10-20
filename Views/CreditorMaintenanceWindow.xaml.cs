using System.Collections.ObjectModel;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class CreditorMaintenanceWindow : FluentWindow
    {
        public ObservableCollection<Creditor> Creditors { get; set; } = new();

        public CreditorMaintenanceWindow()
        {
            InitializeComponent();
            GridCreditors.ItemsSource = Creditors;
            Reload();
        }

        private async void Reload()
        {
            Creditors.Clear();
            var list = await DatabaseService.GetCreditorsAsync();
            foreach (var c in list) Creditors.Add(c);
            if (Creditors.Count > 0) GridCreditors.SelectedIndex = 0;
        }

        private void GridCreditors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridCreditors.SelectedItem is Creditor c)
            {
                TxtCode.Text = c.Code;
                TxtName.Text = c.Name;
                TxtPhone.Text = c.Telephone;
                TxtTerms.Text = c.Terms;
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            TxtCode.Clear();
            TxtName.Clear();
            TxtPhone.Clear();
            TxtTerms.Text = "30 Days";
            TxtCode.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtCode.Text.Trim();
            string name = TxtName.Text.Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            {
                System.Windows.MessageBox.Show("Please enter Supplier Code and Name.", "Validation Error");
                return;
            }

            DatabaseService.SaveCreditor(new Creditor
            {
                Code = code,
                Name = name,
                Telephone = TxtPhone.Text.Trim(),
                Terms = TxtTerms.Text.Trim()
            });

            Reload();
            System.Windows.MessageBox.Show($"Supplier '{name}' saved successfully!", "Saved");
        }
    }
}
