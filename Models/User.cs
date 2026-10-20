namespace FamilySupermarketPOS.Models
{
    public class User
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Cashier"; // Admin, Manager, Cashier
        public bool CanDiscount { get; set; } = true;
        public bool CanDelete { get; set; } = false;
        public bool CanPriceChange { get; set; } = false;
        public bool CanCashUp { get; set; } = true;

        public string DisplayName => $"{Name} ({Role})";
        public string FullName => string.IsNullOrWhiteSpace(Surname) ? Name : $"{Name} {Surname}";
    }
}
