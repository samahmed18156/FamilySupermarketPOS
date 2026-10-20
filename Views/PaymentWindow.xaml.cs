using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public double AmountTendered { get; set; }
        public double Change { get; set; }
    }

    public partial class PaymentWindow : FluentWindow
    {
        private readonly double _grandTotal;
        public string SelectedMethod { get; private set; } = "Cash";
        public double AmountTendered { get; private set; }
        public double ChangeAmount { get; private set; }
        public PaymentResult Result { get; } = new();

        public PaymentWindow(double grandTotal)
        {
            InitializeComponent();
            _grandTotal = Math.Round(grandTotal, 2);
            TxtTotalDue.Text = $"R {_grandTotal:N2}";
            BtnExact.Content = $"Exact (R {_grandTotal:N2})";

            Loaded += (s, e) =>
            {
                TxtTendered.Text = _grandTotal.ToString("0.00", CultureInfo.InvariantCulture);
                TxtTendered.Focus();
                TxtTendered.SelectAll();
                Recalculate();
            };

            PreviewKeyDown += PaymentWindow_PreviewKeyDown;
        }

        private void PaymentWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.F1)
            {
                SelectPaymentMethod("Cash");
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                SelectPaymentMethod("Card");
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                SelectPaymentMethod("Account");
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                FinishSale();
                e.Handled = true;
            }
        }

        private void BtnMethod_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnCash) SelectPaymentMethod("Cash");
            else if (sender == BtnCard) SelectPaymentMethod("Card");
            else if (sender == BtnAccount) SelectPaymentMethod("Account");
        }

        private void SelectPaymentMethod(string method)
        {
            SelectedMethod = method;
            BtnCash.Appearance = method == "Cash" ? ControlAppearance.Primary : ControlAppearance.Secondary;
            BtnCard.Appearance = method == "Card" ? ControlAppearance.Primary : ControlAppearance.Secondary;
            BtnAccount.Appearance = method == "Account" ? ControlAppearance.Primary : ControlAppearance.Secondary;

            if (method != "Cash")
            {
                PanelFastCash.Visibility = Visibility.Collapsed;
                TxtTendered.Text = _grandTotal.ToString("0.00", CultureInfo.InvariantCulture);
                TxtTendered.IsEnabled = false;
            }
            else
            {
                PanelFastCash.Visibility = Visibility.Visible;
                TxtTendered.IsEnabled = true;
                TxtTendered.Focus();
                TxtTendered.SelectAll();
            }
            Recalculate();
        }

        private void BtnFastCash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                if (btn.Tag != null && double.TryParse(btn.Tag.ToString(), out double fastVal))
                {
                    TxtTendered.Text = fastVal.ToString("0.00", CultureInfo.InvariantCulture);
                }
                else
                {
                    // Exact
                    TxtTendered.Text = _grandTotal.ToString("0.00", CultureInfo.InvariantCulture);
                }
                TxtTendered.Focus();
                TxtTendered.SelectAll();
                Recalculate();
            }
        }

        private void TxtTendered_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Recalculate();
        }

        private void TxtTendered_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FinishSale();
                e.Handled = true;
            }
        }

        private void Recalculate()
        {
            string text = TxtTendered.Text.Trim();
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double tendered))
            {
                AmountTendered = tendered;
                ChangeAmount = Math.Round(tendered - _grandTotal, 2);

                if (ChangeAmount >= 0)
                {
                    TxtChange.Text = $"R {ChangeAmount:N2}";
                    TxtChange.Foreground = new SolidColorBrush(Color.FromRgb(16, 124, 65)); // Green
                    BtnComplete.IsEnabled = true;
                }
                else
                {
                    TxtChange.Text = $"-R {Math.Abs(ChangeAmount):N2} (SHORT)";
                    TxtChange.Foreground = new SolidColorBrush(Color.FromRgb(216, 59, 1)); // Orange/Red
                    BtnComplete.IsEnabled = SelectedMethod != "Cash";
                }
            }
            else
            {
                TxtChange.Text = "R 0.00";
                BtnComplete.IsEnabled = false;
            }
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            FinishSale();
        }

        private void FinishSale()
        {
            if (SelectedMethod == "Cash" && AmountTendered < _grandTotal)
            {
                System.Windows.MessageBox.Show("Amount tendered is less than the total due.", "Insufficient Tender");
                TxtTendered.Focus();
                TxtTendered.SelectAll();
                return;
            }

            Result.Success = true;
            Result.PaymentMethod = SelectedMethod;
            Result.AmountTendered = AmountTendered;
            Result.Change = Math.Max(0, ChangeAmount);

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
