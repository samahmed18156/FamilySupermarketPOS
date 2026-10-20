namespace FamilySupermarketPOS.Models
{
    public class Creditor
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Terms { get; set; } = "30 Days";

        public string DisplayName => $"{Code} - {Name}";
    }
}
