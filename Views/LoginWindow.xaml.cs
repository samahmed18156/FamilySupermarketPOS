using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class LoginWindow : FluentWindow
    {
        public User? AuthenticatedUser { get; private set; }
        public User? LoggedInUser => AuthenticatedUser;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                TxtUserId.Focus();
                TxtUserId.SelectAll();
            };
        }

        private void Txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TxtPassword.Focus();
        }

        private async void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await DoLogin();
        }

        private async void QuickLogin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null)
            {
                string[] parts = btn.Tag.ToString()!.Split(':');
                TxtUserId.Text = parts[0];
                TxtPassword.Password = parts[1];
                await DoLogin();
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e) => await DoLogin();

        private async System.Threading.Tasks.Task DoLogin()
        {
            string id = TxtUserId.Text.Trim();
            string pass = TxtPassword.Password;

            var user = await DatabaseService.AuthenticateUserAsync(id, pass);
            if (user != null)
            {
                AuthenticatedUser = user;
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid User ID or Password.", "Authentication Failed");
                TxtPassword.SelectAll();
                TxtPassword.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
