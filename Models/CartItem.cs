namespace FamilySupermarketPOS.Models
{
    public class CartItem
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public double Qty { get; set; } = 1;
        public double PriceExcl { get; set; }
        public double TotalIncl { get; set; }

        public string PriceExclDisplay => $"R {PriceExcl:N2}";
        public string TotalInclDisplay => $"R {TotalIncl:N2}";
    }
}
