using System;
using System.Collections.Generic;

namespace FamilySupermarketPOS.Models
{
    public class Quote
    {
        public string QuoteNumber { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string CustomerAccount { get; set; } = "CASH";
        public string CustomerName { get; set; } = "Cash Sale";
        public double TotalAmount { get; set; }
        public List<CartItem> Items { get; set; } = new();

        public string DisplayDate => Date.ToString("yyyy-MM-dd");
        public string DisplayTotal => $"R {TotalAmount:N2}";
    }
}
