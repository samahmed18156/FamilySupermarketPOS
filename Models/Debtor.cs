namespace FamilySupermarketPOS.Models
{
    public class Debtor
    {
        public string AccountNumber { get; set; } = "CASH";
        public string Name { get; set; } = "Cash Sale";
        public string Telephone { get; set; } = "";
        public double CreditLimit { get; set; }
        public double OutstandingBalance { get; set; }

        public string DisplayBalance => $"R {OutstandingBalance:N2}";
        public string DisplayLimit => $"R {CreditLimit:N2}";
    }
}
