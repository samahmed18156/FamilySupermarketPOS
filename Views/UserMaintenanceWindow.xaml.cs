using System.Collections.ObjectModel;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class UserMaintenanceWindow : FluentWindow
    {
        public ObservableCollection<User> Users { get; set; } = new();

        public UserMaintenanceWindow()
        {
            InitializeComponent();
            GridUsers.ItemsSource = Users;
            Reload();
        }

        private void Reload()
        {
            Users.Clear();
            foreach (var u in DatabaseService.GetUsers()) Users.Add(u);
            if (Users.Count > 0) GridUsers.SelectedIndex = 0;
        }

        private void GridUsers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridUsers.SelectedItem is User u)
            {
                TxtId.Text = u.Id;
                TxtFirstName.Text = u.Name;
                TxtSurname.Text = u.Surname;
                TxtPass.Text = u.Password;
                CmbRole.Text = u.Role;
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            TxtId.Clear();
            TxtFirstName.Clear();
            TxtSurname.Clear();
            TxtPass.Clear();
            TxtId.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string id = TxtId.Text.Trim();
            string name = TxtFirstName.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                System.Windows.MessageBox.Show("Please enter User ID and First Name.", "Validation Error");
                return;
            }

            string role = (CmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Cashier";

            DatabaseService.SaveUser(new User
            {
                Id = id,
                Name = name,
                Surname = TxtSurname.Text.Trim(),
                Password = TxtPass.Text.Trim(),
                Role = role,
                CanDiscount = true,
                CanDelete = role != "Cashier",
                CanPriceChange = role != "Cashier",
                CanCashUp = true
            });

            Reload();
            System.Windows.MessageBox.Show($"User account '{name}' ({id}) saved successfully!", "Saved");
        }
    }
}
