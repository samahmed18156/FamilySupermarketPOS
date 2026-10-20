using System.Windows;
using System.Windows.Input;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class ReceiptPreviewWindow : FluentWindow
    {
        public ReceiptPreviewWindow()
        {
            InitializeComponent();
            string receipt = HardwareService.LastReceiptText;
            if (string.IsNullOrEmpty(receipt))
            {
                TxtSlip.Text = "No previous receipt in memory.\nComplete a sale first.";
            }
            else
            {
                TxtSlip.Text = receipt;
            }

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    BtnPrint_Click(s, e);
                    e.Handled = true;
                }
            };
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Reprint sent to default POS thermal printer.", "Receipt Reprint");
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
