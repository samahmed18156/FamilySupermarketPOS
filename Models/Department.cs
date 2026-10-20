namespace FamilySupermarketPOS.Models
{
    public class Department
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        public string DisplayName => $"{Code} - {Name}";
    }
}
