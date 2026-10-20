using System.Windows;
using System.Windows.Media;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class DatabaseConfigWindow : FluentWindow
    {
        public bool ConfigurationSaved { get; private set; }

        public DatabaseConfigWindow()
        {
            InitializeComponent();
            var cfg = ConfigService.Current;
            TxtHost.Text = cfg.ServerHost;
            TxtPort.Text = cfg.ServerPort.ToString();
            TxtDatabase.Text = cfg.DatabaseName;
            TxtUser.Text = cfg.Username;
            TxtPassword.Password = cfg.Password;
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = "Testing connection to MySQL...";
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 212));

            string host = TxtHost.Text.Trim();
            int.TryParse(TxtPort.Text.Trim(), out int port);
            if (port <= 0) port = 3306;
            string db = TxtDatabase.Text.Trim();
            string user = TxtUser.Text.Trim();
            string pass = TxtPassword.Password;

            string testStr = $"Server={host};Port={port};Database={db};User ID={user};Password={pass};AllowUserVariables=True;";
            bool ok = await DatabaseService.TestConnectionAsync(testStr);

            if (ok)
            {
                TxtStatus.Text = "Connection successful! MySQL server is reachable.";
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(16, 124, 65));
            }
            else
            {
                TxtStatus.Text = "Connection failed! Check host, port, user or password.";
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(216, 59, 1));
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(TxtPort.Text.Trim(), out int port);
            if (port <= 0) port = 3306;

            var cfg = ConfigService.Current;
            cfg.ServerHost = TxtHost.Text.Trim();
            cfg.ServerPort = port;
            cfg.DatabaseName = TxtDatabase.Text.Trim();
            cfg.Username = TxtUser.Text.Trim();
            cfg.Password = TxtPassword.Password;

            ConfigService.Save();

            bool ok = await DatabaseService.TestConnectionAsync();
            ConfigurationSaved = true;

            if (ok)
            {
                System.Windows.MessageBox.Show("Settings saved! Connected to MySQL database.", "Blue_Knight POS");
            }
            else
            {
                System.Windows.MessageBox.Show("Settings saved to config.json. (Server not currently reachable).", "Blue_Knight POS");
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
