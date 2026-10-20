namespace FamilySupermarketPOS.Models
{
    public class Product
    {
        public string Code { get; set; } = "";
        public string Barcode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Department { get; set; } = "";
        public double CostExcl { get; set; }
        public double SellingPriceIncl { get; set; }
        public double SellingPriceExcl => System.Math.Round(SellingPriceIncl / 1.15, 2);
        public double Soh { get; set; }

        public string SellingPriceDisplay => $"R {SellingPriceIncl:N2}";
        public string CostDisplay => $"R {CostExcl:N2}";
        public string SohDisplay => $"{Soh:N0}";
    }
}
