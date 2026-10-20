using System;

namespace FamilySupermarketPOS.Models
{
    public class PettyCashItem
    {
        public string VoucherNo { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Category { get; set; } = "General Expense";
        public string Description { get; set; } = "";
        public double Amount { get; set; }
        public string AuthorizedBy { get; set; } = "Manager";

        public string DisplayAmount => $"R {Amount:N2}";
        public string DisplayDate => Date.ToString("yyyy-MM-dd HH:mm");
    }
}
