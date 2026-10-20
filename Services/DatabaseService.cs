using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FamilySupermarketPOS.Models;
using MySqlConnector;

namespace FamilySupermarketPOS.Services
{
    public class DatabaseService
    {
        // Default local MySQL credentials for MiPOS store database
        public static string ConnectionString { get; set; } = 
            "Server=localhost;Database=store;User ID=root;Password=;Port=3306;AllowUserVariables=True;";

        // Built-in demo items if MySQL server is not currently running
        private static readonly List<Product> DemoProducts = new()
        {
            new Product { Code = "1001", Barcode = "6001001001001", Description = "Full Cream Milk 2L", Department = "DAIRY", CostExcl = 26.50, SellingPriceIncl = 32.99, Soh = 48 },
            new Product { Code = "1002", Barcode = "6001002002002", Description = "White Bread 700g", Department = "BAKERY", CostExcl = 14.80, SellingPriceIncl = 18.50, Soh = 60 },
            new Product { Code = "1003", Barcode = "6001003003003", Description = "Sunflower Cooking Oil 2L", Department = "GROCERY", CostExcl = 65.00, SellingPriceIncl = 79.99, Soh = 24 },
            new Product { Code = "1004", Barcode = "6001004004004", Description = "White Sugar 2.5kg", Department = "GROCERY", CostExcl = 44.00, SellingPriceIncl = 54.00, Soh = 35 },
            new Product { Code = "1005", Barcode = "6001005005005", Description = "Coca Cola 2L", Department = "BEVERAGES", CostExcl = 21.00, SellingPriceIncl = 26.99, Soh = 72 },
            new Product { Code = "1006", Barcode = "6001006006006", Description = "Large Eggs 30-Pack", Department = "PERISHABLES", CostExcl = 72.00, SellingPriceIncl = 89.99, Soh = 15 },
            new Product { Code = "1007", Barcode = "6001007007007", Description = "Parboiled Rice 2kg", Department = "GROCERY", CostExcl = 31.00, SellingPriceIncl = 38.50, Soh = 40 },
            new Product { Code = "1008", Barcode = "6001008008008", Description = "Maize Meal 5kg", Department = "GROCERY", CostExcl = 52.00, SellingPriceIncl = 64.99, Soh = 30 }
        };

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

                string sql = @"
                    SELECT code, descr, dept, cpe, sp1, soh, skunum 
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
                        Barcode = reader["skunum"].ToString()?.Trim('|') ?? "",
                        CostExcl = Convert.ToDouble(reader["cpe"] == DBNull.Value ? 0 : reader["cpe"]),
                        SellingPriceIncl = Convert.ToDouble(reader["sp1"] == DBNull.Value ? 0 : reader["sp1"]),
                        Soh = Convert.ToDouble(reader["soh"] == DBNull.Value ? 0 : reader["soh"])
                    };
                }
            }
            catch
            {
                // Fall back to demo items
            }

            // Fallback demo search
            return DemoProducts.Find(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) || 
                                          p.Barcode.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            var results = new List<Product>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT code, descr, dept, cpe, sp1, soh, skunum 
                    FROM stockt2 
                    WHERE code LIKE @term OR descr LIKE @term OR skunum LIKE @term 
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
                        Barcode = reader["skunum"].ToString()?.Trim('|') ?? "",
                        CostExcl = Convert.ToDouble(reader["cpe"] == DBNull.Value ? 0 : reader["cpe"]),
                        SellingPriceIncl = Convert.ToDouble(reader["sp1"] == DBNull.Value ? 0 : reader["sp1"]),
                        Soh = Convert.ToDouble(reader["soh"] == DBNull.Value ? 0 : reader["soh"])
                    });
                }

                if (results.Count > 0) return results;
            }
            catch
            {
                // Fall back to demo items
            }

            // Fallback demo search
            string term = searchTerm.ToLowerInvariant();
            return DemoProducts.FindAll(p => p.Code.ToLowerInvariant().Contains(term) || 
                                             p.Description.ToLowerInvariant().Contains(term) ||
                                             p.Department.ToLowerInvariant().Contains(term));
        }

        public static async Task<bool> SaveStockItemAsync(Product item, bool isNew)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string barcodeFormatted = string.IsNullOrEmpty(item.Barcode) ? "NONE" : $"|{item.Barcode}|";

                string sql;
                if (isNew)
                {
                    sql = @"
                        INSERT INTO stockt2 (code, descr, dept, cpe, sp1, soh, skunum) 
                        VALUES (@code, @descr, @dept, @cpe, @sp1, @soh, @skunum);";
                }
                else
                {
                    sql = @"
                        UPDATE stockt2 
                        SET descr = @descr, dept = @dept, cpe = @cpe, sp1 = @sp1, soh = @soh, skunum = @skunum 
                        WHERE code = @code;";
                }

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", item.Code);
                cmd.Parameters.AddWithValue("@descr", item.Description);
                cmd.Parameters.AddWithValue("@dept", item.Department);
                cmd.Parameters.AddWithValue("@cpe", item.CostExcl);
                cmd.Parameters.AddWithValue("@sp1", item.SellingPriceIncl);
                cmd.Parameters.AddWithValue("@soh", item.Soh);
                cmd.Parameters.AddWithValue("@skunum", barcodeFormatted);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveStock error: {ex.Message}");
                // Also update local demo list
                int idx = DemoProducts.FindIndex(p => p.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) DemoProducts[idx] = item;
                else DemoProducts.Add(item);
                return true;
            }
        }

        public static async Task<bool> RecordSaleTransactionAsync(string invoiceNo, List<CartItem> items, string paymentMethod)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                foreach (var item in items)
                {
                    // Deduct stock on hand
                    string updateSoh = "UPDATE stockt2 SET soh = soh - @qty WHERE code = @code;";
                    using (var cmd = new MySqlCommand(updateSoh, conn))
                    {
                        cmd.Parameters.AddWithValue("@qty", item.Qty);
                        cmd.Parameters.AddWithValue("@code", item.Code);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert sales transaction into m1sttrans if it exists
                    string insertTrans = @"
                        INSERT INTO m1sttrans (date, code, qty, transtype, docno, sp, vat) 
                        VALUES (NOW(), @code, @qty * -1, '0', @docno, @sp, @vat);";
                    try
                    {
                        using var cmd = new MySqlCommand(insertTrans, conn);
                        cmd.Parameters.AddWithValue("@code", item.Code);
                        cmd.Parameters.AddWithValue("@qty", item.Qty);
                        cmd.Parameters.AddWithValue("@docno", invoiceNo);
                        cmd.Parameters.AddWithValue("@sp", item.TotalIncl);
                        cmd.Parameters.AddWithValue("@vat", item.TotalIncl - item.PriceExcl);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        // Ignore if table doesn't match exactly yet
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
