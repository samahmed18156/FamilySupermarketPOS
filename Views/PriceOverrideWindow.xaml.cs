using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class PriceOverrideWindow : FluentWindow
    {
        public double NewPrice { get; private set; }

        public PriceOverrideWindow(string itemDescr, double currentPrice)
        {
            InitializeComponent();
            TxtItemDescr.Text = $"Item: {itemDescr}";
            TxtCurrentPrice.Text = $"Current Price: R {currentPrice:N2}";
            TxtNewPrice.Text = currentPrice.ToString("0.00", CultureInfo.InvariantCulture);

            Loaded += (s, e) =>
            {
                TxtNewPrice.Focus();
                TxtNewPrice.SelectAll();
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    BtnUpdate_Click(s, e);
                    e.Handled = true;
                }
            };
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtNewPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double price) && price >= 0)
            {
                NewPrice = price;
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid price.", "Invalid Price");
                TxtNewPrice.SelectAll();
                TxtNewPrice.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
