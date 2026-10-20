using System;

namespace FamilySupermarketPOS.Models
{
    public class Laybye
    {
        public string LaybyeNo { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public double TotalAmount { get; set; }
        public double AmountPaid { get; set; }
        public double Balance => Math.Max(0, TotalAmount - AmountPaid);
        public string Status { get; set; } = "Active"; // Active, PaidInFull, Cancelled
        public string ItemsSummary { get; set; } = "";

        public string DisplayTotal => $"R {TotalAmount:N2}";
        public string DisplayPaid => $"R {AmountPaid:N2}";
        public string DisplayBalance => $"R {Balance:N2}";
        public string DisplayDate => Date.ToString("yyyy-MM-dd");
    }
}
