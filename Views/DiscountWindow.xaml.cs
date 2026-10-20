using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class DiscountWindow : FluentWindow
    {
        public double DiscountPercent { get; private set; }
        public bool ApplyToAll { get; private set; }

        public DiscountWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                TxtPercent.Focus();
                TxtPercent.SelectAll();
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
                    BtnApply_Click(s, e);
                    e.Handled = true;
                }
            };
        }

        public DiscountWindow(double currentTotal) : this()
        {
        }

        private void BtnFastPct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null)
            {
                TxtPercent.Text = btn.Tag.ToString();
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtPercent.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double pct) && pct >= 0 && pct <= 100)
            {
                DiscountPercent = pct;
                ApplyToAll = RadioAll.IsChecked == true;
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid percentage between 0 and 100.", "Invalid Percentage");
                TxtPercent.SelectAll();
                TxtPercent.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
