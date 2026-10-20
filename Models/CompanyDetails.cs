namespace FamilySupermarketPOS.Models
{
    public class CompanyDetails
    {
        public string CompanyName { get; set; } = "BLUE_KNIGHT SUPERMARKET";
        public string Address1 { get; set; } = "160 MAIN REEF ROAD";
        public string Address2 { get; set; } = "CROWN MINES";
        public string City { get; set; } = "JHB";
        public string PostalCode { get; set; } = "2000";
        public string Telephone { get; set; } = "(011) 830-2722";
        public string VatNumber { get; set; } = "4710192837";
        public double VatRate { get; set; } = 0.15;
        public string ReceiptMessage { get; set; } = "THANK YOU FOR SHOPPING WITH US";
    }
}
