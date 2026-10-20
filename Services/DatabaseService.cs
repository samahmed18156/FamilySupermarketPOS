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
        public static string ConnectionString => ConfigService.Current.ToConnectionString();

        // Built-in Demo Products
        private static readonly List<Product> DemoProducts = new()
        {
            new Product { Code = "1001", Barcode = "6001001001001", Description = "Full Cream Milk 2L", Department = "DAIRY", CostExcl = 26.50, SellingPriceIncl = 32.99, Soh = 48 },
            new Product { Code = "1002", Barcode = "6001002002002", Description = "White Bread 700g", Department = "BAKERY", CostExcl = 14.80, SellingPriceIncl = 18.50, Soh = 60 },
            new Product { Code = "1003", Barcode = "6001003003003", Description = "Sunflower Cooking Oil 2L", Department = "GROCERY", CostExcl = 65.00, SellingPriceIncl = 79.99, Soh = 24 },
            new Product { Code = "1004", Barcode = "6001004004004", Description = "White Sugar 2.5kg", Department = "GROCERY", CostExcl = 44.00, SellingPriceIncl = 54.00, Soh = 35 },
            new Product { Code = "1005", Barcode = "6001005005005", Description = "Coca Cola 2L", Department = "BEVERAGES", CostExcl = 21.00, SellingPriceIncl = 26.99, Soh = 72 },
            new Product { Code = "1006", Barcode = "6001006006006", Description = "Large Eggs 30-Pack", Department = "PERISHABLES", CostExcl = 72.00, SellingPriceIncl = 89.99, Soh = 0 },
            new Product { Code = "1007", Barcode = "6001007007007", Description = "Parboiled Rice 2kg", Department = "GROCERY", CostExcl = 31.00, SellingPriceIncl = 38.50, Soh = 40 },
            new Product { Code = "1008", Barcode = "6001008008008", Description = "Maize Meal 5kg", Department = "GROCERY", CostExcl = 52.00, SellingPriceIncl = 64.99, Soh = 0 }
        };

        // Built-in Demo Debtors
        private static readonly List<Debtor> DemoDebtors = new()
        {
            new Debtor { AccountNumber = "CASH", Name = "Cash Sale", Telephone = "-", CreditLimit = 0, OutstandingBalance = 0 },
            new Debtor { AccountNumber = "ACC001", Name = "Ahmed Family Store", Telephone = "082 123 4567", CreditLimit = 15000, OutstandingBalance = 2450.00 },
            new Debtor { AccountNumber = "ACC002", Name = "Zaman Enterprises", Telephone = "071 987 6543", CreditLimit = 25000, OutstandingBalance = 8750.50 },
            new Debtor { AccountNumber = "ACC003", Name = "Retreat Wholesale", Telephone = "021 701 0000", CreditLimit = 50000, OutstandingBalance = 14200.00 }
        };

        // Built-in Demo Creditors
        private static readonly List<Creditor> DemoCreditors = new()
        {
            new Creditor { Code = "CLO001", Name = "Clover SA (Pty) Ltd", Telephone = "011 471 1400", Address = "Johannesburg", Terms = "30 Days" },
            new Creditor { Code = "PRE001", Name = "Premier FMCG (Blue Ribbon)", Telephone = "011 565 8000", Address = "Epping, Cape Town", Terms = "7 Days" },
            new Creditor { Code = "COC001", Name = "Coca-Cola Beverages SA", Telephone = "021 930 9000", Address = "Parow, Cape Town", Terms = "14 Days" },
            new Creditor { Code = "PIO001", Name = "Pioneer Foods (Sasko)", Telephone = "021 807 5100", Address = "Paarl, Western Cape", Terms = "30 Days" }
        };

        // Built-in Demo Users (matching m1users in DB.DAT)
        private static readonly List<User> DemoUsers = new()
        {
            new User { Id = "adm", Name = "Admin", Surname = "User", Password = "n", Role = "Admin", CanDiscount = true, CanDelete = true, CanPriceChange = true, CanCashUp = true },
            new User { Id = "MAN", Name = "Manager", Surname = "User", Password = "123", Role = "Manager", CanDiscount = true, CanDelete = true, CanPriceChange = true, CanCashUp = true },
            new User { Id = "CASH1", Name = "ZAMAN", Surname = "MP", Password = "1", Role = "Cashier", CanDiscount = true, CanDelete = false, CanPriceChange = false, CanCashUp = true }
        };

        // Built-in Demo Departments
        private static readonly List<Department> DemoDepartments = new()
        {
            new Department { Code = "GROCERY", Name = "Dry Groceries & Staples" },
            new Department { Code = "DAIRY", Name = "Milk, Cheeses & Yogurts" },
            new Department { Code = "BAKERY", Name = "Bread, Rolls & Pastries" },
            new Department { Code = "BEVERAGES", Name = "Cool Drinks & Juices" },
            new Department { Code = "PERISHABLES", Name = "Eggs, Butter & Chilled" },
            new Department { Code = "BUTCHERY", Name = "Meat & Poultry" },
            new Department { Code = "PRODUCE", Name = "Fruit & Vegetables" },
            new Department { Code = "CLEANING", Name = "Detergents & Toiletries" }
        };

        private static readonly List<Laybye> DemoLaybyes = new()
        {
            new Laybye { LaybyeNo = "LB-1001", CustomerName = "Fatima Hendricks", CustomerPhone = "083 456 7890", TotalAmount = 1450.00, AmountPaid = 800.00, ItemsSummary = "Blankets & Cookware Set" },
            new Laybye { LaybyeNo = "LB-1002", CustomerName = "Kareem Adams", CustomerPhone = "072 111 2233", TotalAmount = 980.00, AmountPaid = 300.00, ItemsSummary = "Groceries Bulk Hamper" }
        };

        private static readonly List<PettyCashItem> DemoPettyCash = new()
        {
            new PettyCashItem { VoucherNo = "PC-001", Category = "Store Supplies", Description = "Till paper rolls & cleaning soap", Amount = 120.00, AuthorizedBy = "MAN" }
        };

        private static readonly List<Quote> DemoQuotes = new();

        public static async Task<bool> TestConnectionAsync(string? testConnStr = null)
        {
            try
            {
                using var conn = new MySqlConnection(testConnStr ?? ConnectionString);
                await conn.OpenAsync();
                return conn.State == ConnectionState.Open;
            }
            catch { return false; }
        }

        // ==========================================
        // USERS & AUTHENTICATION
        // ==========================================
        public static async Task<User?> AuthenticateUserAsync(string userId, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = "SELECT id, name, surname, password FROM m1users WHERE id = @id AND password = @pass LIMIT 1;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pass", password);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string id = reader["id"].ToString() ?? "";
                    string role = id.Equals("adm", StringComparison.OrdinalIgnoreCase) ? "Admin" : 
                                 id.Equals("MAN", StringComparison.OrdinalIgnoreCase) ? "Manager" : "Cashier";

                    return new User
                    {
                        Id = id,
                        Name = reader["name"].ToString() ?? "",
                        Surname = reader["surname"].ToString() ?? "",
                        Role = role,
                        CanDiscount = true,
                        CanDelete = role != "Cashier",
                        CanPriceChange = role != "Cashier",
                        CanCashUp = true
                    };
                }
            }
            catch { }

            return DemoUsers.Find(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase) && u.Password == password);
        }

        public static List<User> GetUsers() => DemoUsers;

        public static void SaveUser(User user)
        {
            int idx = DemoUsers.FindIndex(u => u.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoUsers[idx] = user;
            else DemoUsers.Add(user);
        }

        // ==========================================
        // PRODUCTS & STOCK
        // ==========================================
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
            catch { }

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
                    LIMIT 80;";

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
            catch { }

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

                string sql = isNew
                    ? @"INSERT INTO stockt2 (code, descr, dept, cpe, sp1, soh, skunum) 
                        VALUES (@code, @descr, @dept, @cpe, @sp1, @soh, @skunum);"
                    : @"UPDATE stockt2 
                        SET descr = @descr, dept = @dept, cpe = @cpe, sp1 = @sp1, soh = @soh, skunum = @skunum 
                        WHERE code = @code;";

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
            catch
            {
                int idx = DemoProducts.FindIndex(p => p.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) DemoProducts[idx] = item;
                else DemoProducts.Add(item);
                return true;
            }
        }

        public static async Task<bool> AdjustStockAsync(string code, double qtyChange, string reason)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = "UPDATE stockt2 SET soh = soh + @qty WHERE code = @code;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@qty", qtyChange);
                cmd.Parameters.AddWithValue("@code", code);
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                var prod = DemoProducts.Find(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (prod != null)
                {
                    prod.Soh += qtyChange;
                    return true;
                }
                return false;
            }
        }

        // ==========================================
        // DEPARTMENTS
        // ==========================================
        public static List<Department> GetDepartments() => DemoDepartments;

        public static void SaveDepartment(Department dept)
        {
            int idx = DemoDepartments.FindIndex(d => d.Code.Equals(dept.Code, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoDepartments[idx] = dept;
            else DemoDepartments.Add(dept);
        }

        public static void DeleteDepartment(string code)
        {
            DemoDepartments.RemoveAll(d => d.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        // ==========================================
        // CREDITORS (SUPPLIERS)
        // ==========================================
        public static async Task<List<Creditor>> GetCreditorsAsync(string query = "")
        {
            var list = new List<Creditor>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = "SELECT cnum, name, tel1, add1, terms FROM m1creditors WHERE cnum LIKE @term OR name LIKE @term ORDER BY name ASC;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@term", $"%{query}%");

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Creditor
                    {
                        Code = reader["cnum"].ToString() ?? "",
                        Name = reader["name"].ToString() ?? "",
                        Telephone = reader["tel1"].ToString() ?? "",
                        Address = reader["add1"].ToString() ?? "",
                        Terms = reader["terms"].ToString() ?? "30 Days"
                    });
                }
                if (list.Count > 0) return list;
            }
            catch { }

            string term = query.ToLowerInvariant();
            return DemoCreditors.FindAll(c => c.Code.ToLowerInvariant().Contains(term) || c.Name.ToLowerInvariant().Contains(term));
        }

        public static void SaveCreditor(Creditor cred)
        {
            int idx = DemoCreditors.FindIndex(c => c.Code.Equals(cred.Code, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoCreditors[idx] = cred;
            else DemoCreditors.Add(cred);
        }

        public static async Task<bool> ProcessGrnAsync(string grnNo, string supplierCode, string supplierInv, List<CartItem> items)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                foreach (var item in items)
                {
                    string updateSql = "UPDATE stockt2 SET soh = soh + @qty, cpe = @cost WHERE code = @code;";
                    using var cmd = new MySqlCommand(updateSql, conn);
                    cmd.Parameters.AddWithValue("@qty", item.Qty);
                    cmd.Parameters.AddWithValue("@cost", item.PriceExcl);
                    cmd.Parameters.AddWithValue("@code", item.Code);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch
            {
                foreach (var item in items)
                {
                    var prod = DemoProducts.Find(p => p.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                    if (prod != null)
                    {
                        prod.Soh += item.Qty;
                        prod.CostExcl = item.PriceExcl;
                    }
                }
                return true;
            }
        }

        // ==========================================
        // DEBTORS & CUSTOMER ACCOUNTS
        // ==========================================
        public static async Task<List<Debtor>> SearchDebtorsAsync(string query)
        {
            var list = new List<Debtor>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT accnum, name, tel1, crlimit, osbal 
                    FROM m1debtors 
                    WHERE accnum LIKE @term OR name LIKE @term 
                    ORDER BY name ASC 
                    LIMIT 30;";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@term", $"%{query}%");

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Debtor
                    {
                        AccountNumber = reader["accnum"].ToString() ?? "",
                        Name = reader["name"].ToString() ?? "",
                        Telephone = reader["tel1"].ToString() ?? "",
                        CreditLimit = Convert.ToDouble(reader["crlimit"] == DBNull.Value ? 0 : reader["crlimit"]),
                        OutstandingBalance = Convert.ToDouble(reader["osbal"] == DBNull.Value ? 0 : reader["osbal"])
                    });
                }
                if (list.Count > 0) return list;
            }
            catch { }

            string term = query.ToLowerInvariant();
            return DemoDebtors.FindAll(d => d.AccountNumber.ToLowerInvariant().Contains(term) || 
                                           d.Name.ToLowerInvariant().Contains(term));
        }

        public static void SaveDebtor(Debtor d)
        {
            int idx = DemoDebtors.FindIndex(x => x.AccountNumber.Equals(d.AccountNumber, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoDebtors[idx] = d;
            else DemoDebtors.Add(d);
        }

        // ==========================================
        // QUOTATIONS
        // ==========================================
        public static List<Quote> GetQuotes() => DemoQuotes;

        public static void SaveQuote(Quote q)
        {
            int idx = DemoQuotes.FindIndex(x => x.QuoteNumber.Equals(q.QuoteNumber, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoQuotes[idx] = q;
            else DemoQuotes.Add(q);
        }

        // ==========================================
        // LAYBYES & PETTY CASH
        // ==========================================
        public static List<Laybye> GetLaybyes() => DemoLaybyes;

        public static void SaveLaybye(Laybye lb)
        {
            int idx = DemoLaybyes.FindIndex(x => x.LaybyeNo.Equals(lb.LaybyeNo, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) DemoLaybyes[idx] = lb;
            else DemoLaybyes.Add(lb);
        }

        public static List<PettyCashItem> GetPettyCashList() => DemoPettyCash;

        public static void SavePettyCash(PettyCashItem item) => DemoPettyCash.Add(item);

        // ==========================================
        // REPORTS
        // ==========================================
        public static async Task<List<Product>> GetZeroSohReportAsync()
        {
            var list = new List<Product>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = "SELECT code, descr, dept, cpe, sp1, soh FROM stockt2 WHERE soh <= 0 ORDER BY dept ASC, descr ASC;";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Product
                    {
                        Code = reader["code"].ToString() ?? "",
                        Description = reader["descr"].ToString() ?? "",
                        Department = reader["dept"].ToString() ?? "",
                        CostExcl = Convert.ToDouble(reader["cpe"] == DBNull.Value ? 0 : reader["cpe"]),
                        SellingPriceIncl = Convert.ToDouble(reader["sp1"] == DBNull.Value ? 0 : reader["sp1"]),
                        Soh = Convert.ToDouble(reader["soh"] == DBNull.Value ? 0 : reader["soh"])
                    });
                }
                if (list.Count > 0) return list;
            }
            catch { }

            return DemoProducts.FindAll(p => p.Soh <= 0);
        }

        public static async Task<List<Product>> GetAllStockForValuationAsync() => await SearchProductsAsync("");

        public static async Task<bool> RecordSaleTransactionAsync(string invoiceNo, List<CartItem> items, string paymentMethod)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                foreach (var item in items)
                {
                    // Negative qty on returns adds back to stock!
                    string updateSoh = "UPDATE stockt2 SET soh = soh - @qty WHERE code = @code;";
                    using var cmd = new MySqlCommand(updateSoh, conn);
                    cmd.Parameters.AddWithValue("@qty", item.Qty);
                    cmd.Parameters.AddWithValue("@code", item.Code);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch
            {
                foreach (var item in items)
                {
                    var prod = DemoProducts.Find(p => p.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                    if (prod != null) prod.Soh -= item.Qty;
                }
                return true;
            }
        }

        public static async Task<bool> RestituteStockAsync(string creditNo, List<CartItem> items)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                await conn.OpenAsync();

                foreach (var item in items)
                {
                    double returnQty = Math.Abs(item.Qty);
                    string updateSoh = "UPDATE stockt2 SET soh = soh + @qty WHERE code = @code;";
                    using var cmd = new MySqlCommand(updateSoh, conn);
                    cmd.Parameters.AddWithValue("@qty", returnQty);
                    cmd.Parameters.AddWithValue("@code", item.Code);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch
            {
                foreach (var item in items)
                {
                    var prod = DemoProducts.Find(p => p.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                    if (prod != null) prod.Soh += Math.Abs(item.Qty);
                }
                return true;
            }
        }
    }
}
