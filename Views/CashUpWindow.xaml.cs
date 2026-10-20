using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class CashUpWindow : FluentWindow
    {
        private double _totalCounted = 0;
        private double _float = 500.00;

        public CashUpWindow()
        {
            InitializeComponent();
            TxtShiftDate.Text = DateTime.Now.ToString("dd MMMM yyyy HH:mm");
            Recalculate();
        }

        private void Denom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            if (TxtPhysicalCash == null) return;

            int.TryParse(Cnt200?.Text.Trim(), out int n200);
            int.TryParse(Cnt100?.Text.Trim(), out int n100);
            int.TryParse(Cnt50?.Text.Trim(), out int n50);
            int.TryParse(Cnt20?.Text.Trim(), out int n20);
            int.TryParse(Cnt10?.Text.Trim(), out int n10);

            int.TryParse(Cnt5?.Text.Trim(), out int c5);
            int.TryParse(Cnt2?.Text.Trim(), out int c2);
            int.TryParse(Cnt1?.Text.Trim(), out int c1);
            int.TryParse(Cnt050?.Text.Trim(), out int c050);

            double.TryParse(TxtOpeningFloat?.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out _float);

            _totalCounted = (n200 * 200.0) + (n100 * 100.0) + (n50 * 50.0) + (n20 * 20.0) + (n10 * 10.0)
                          + (c5 * 5.0) + (c2 * 2.0) + (c1 * 1.0) + (c050 * 0.50);

            double netSales = Math.Max(0, _totalCounted - _float);

            TxtPhysicalCash.Text = $"R {_totalCounted:N2}";
            TxtFloatDisplay.Text = $"R {_float:N2}";
            TxtNetSales.Text = $"R {netSales:N2}";
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            const int width = 40;

            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine("        BLUE_KNIGHT SUPERMARKET        ");
            sb.AppendLine("          CASH UP REPORT SLIP          ");
            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine($"Date:       {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Cashier:    ZAMAN MP (Station M1)");
            sb.AppendLine("".PadRight(width, '-'));
            sb.AppendLine($"Total Physical Cash:  R {_totalCounted,10:N2}");
            sb.AppendLine($"Opening Float:        R {_float,10:N2}");
            sb.AppendLine($"Net Cash Shift Sales: R {Math.Max(0, _totalCounted - _float),10:N2}");
            sb.AppendLine("".PadRight(width, '='));
            sb.AppendLine("Cashier Signature: ___________________");
            sb.AppendLine("Manager Signature: ___________________");
            sb.AppendLine("".PadRight(width, '='));

            string slip = sb.ToString();

            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"CashUp-{DateTime.Now:yyyyMMdd-HHmm}.txt");
                File.WriteAllText(filePath, slip);
            }
            catch { }

            System.Windows.MessageBox.Show(
                $"Cash Up Complete!\n\nTotal Cash Counted: R {_totalCounted:N2}\nNet Cash Sales: R {Math.Max(0, _totalCounted - _float):N2}\n\n[Printed to /Receipts/ folder]",
                "Cash Up Saved");

            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
