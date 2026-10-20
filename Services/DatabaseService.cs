using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;

namespace FamilySupermarketPOS.Services
{
    public class Product
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public string Department { get; set; } = "";
        public double CostExcl { get; set; }
        public double SellingPriceIncl { get; set; }
        public double Soh { get; set; } // Stock on hand
    }

    public class DatabaseService
    {
        // Adjust password/user if your local MySQL settings differ
        private const string ConnectionString = "Server=localhost;Database=store;User ID=root;Password=;Port=3306;AllowUserVariables=True;";

        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();
                return conn.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<Product?> GetProductByCodeOrBarcodeAsync(string code)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Checks item code or barcode in stockt2
                string sql = @"
                    SELECT code, descr, dept, cpe, sp1, soh 
                    FROM stockt2 
                    WHERE code = @code OR skunum LIKE @skunum 
                    LIMIT 1;";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@skunum", $"%|{code}|%");

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Product
                    {
                        Code = reader["code"].ToString() ?? "",
                        Description = reader["descr"].ToString() ?? "",
                        Department = reader["dept"].ToString() ?? "",
                        CostExcl = Convert.ToDouble(reader["cpe"] == DBNull.Value ? 0 : reader["cpe"]),
                        SellingPriceIncl = Convert.ToDouble(reader["sp1"] == DBNull.Value ? 0 : reader["sp1"]),
                        Soh = Convert.ToDouble(reader["soh"] == DBNull.Value ? 0 : reader["soh"])
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Error: {ex.Message}");
            }

            return null;
        }

        public static async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            var results = new List<Product>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT code, descr, dept, cpe, sp1, soh 
                    FROM stockt2 
                    WHERE code LIKE @term OR descr LIKE @term 
                    ORDER BY descr ASC 
                    LIMIT 50;";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@term", $"%{searchTerm}%");

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new Product
                    {
                        Code = reader["code"].ToString() ?? "",
                        Description = reader["descr"].ToString() ?? "",
                        Department = reader["dept"].ToString() ?? "",
                        CostExcl = Convert.ToDouble(reader["cpe"] == DBNull.Value ? 0 : reader["cpe"]),
                        SellingPriceIncl = Convert.ToDouble(reader["sp1"] == DBNull.Value ? 0 : reader["sp1"]),
                        Soh = Convert.ToDouble(reader["soh"] == DBNull.Value ? 0 : reader["soh"])
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Search Error: {ex.Message}");
            }

            return results;
        }
    }
}