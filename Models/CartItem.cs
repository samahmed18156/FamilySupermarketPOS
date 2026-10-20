using System;

namespace FamilySupermarketPOS.Models
{
    public class CartItem
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public double Qty { get; set; } = 1;
        public double UnitPriceIncl { get; set; }
        public double PriceExcl { get; set; }
        public double TotalIncl { get; set; }
        public double DiscountPercent { get; set; } = 0;
        public double Discount { get => DiscountPercent; set => DiscountPercent = value; }
        public bool IsReturn { get; set; } = false;

        public string PriceExclDisplay => $"R {PriceExcl:N2}";
        public string TotalInclDisplay => $"R {TotalIncl:N2}";
        public string DiscountDisplay => DiscountPercent > 0 ? $"{DiscountPercent}% Off" : "-";
        public string QtyDisplay => IsReturn ? $"-{Math.Abs(Qty):N0} (RETURN)" : $"{Qty:N0}";
    }
}
