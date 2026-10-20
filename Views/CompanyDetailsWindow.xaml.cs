using System.Windows;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class CompanyDetailsWindow : FluentWindow
    {
        public CompanyDetailsWindow()
        {
            InitializeComponent();
            var cfg = ConfigService.Current;
            TxtName.Text = cfg.StoreName;
            TxtAddress.Text = "160 MAIN REEF ROAD";
            TxtCity.Text = "JOHANNESBURG 2000";
            TxtPhone.Text = "(011) 830-2722";
            TxtVat.Text = "4710192837";
            TxtFooter.Text = "THANK YOU FOR SHOPPING WITH US";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var cfg = ConfigService.Current;
            cfg.StoreName = TxtName.Text.Trim();
            ConfigService.Save();

            System.Windows.MessageBox.Show("Company details saved successfully!", "Saved");
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
