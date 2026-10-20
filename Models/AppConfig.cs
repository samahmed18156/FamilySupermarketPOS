namespace FamilySupermarketPOS.Models
{
    public class AppConfig
    {
        public string ServerHost { get; set; } = "localhost";
        public int ServerPort { get; set; } = 3306;
        public string DatabaseName { get; set; } = "store";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
        public string StoreName { get; set; } = "BLUE_KNIGHT SUPERMARKET";
        public string StationId { get; set; } = "M1";
        public string CashierName { get; set; } = "ZAMAN MP";

        public string ToConnectionString()
        {
            return $"Server={ServerHost};Port={ServerPort};Database={DatabaseName};User ID={Username};Password={Password};AllowUserVariables=True;";
        }
    }
}
