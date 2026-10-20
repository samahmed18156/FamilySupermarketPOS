# Blue_Knight POS - Complete Retail & Supermarket Terminal Suite

Modern, ultra-responsive supermarket and retail Point of Sale system built with **C# .NET 8** and **WPF-UI Fluent Design**. Designed as a complete, drop-in replacement for the legacy MiPOS Java desktop system with 100% feature parity, running efficiently on 4 GB RAM Windows 10/11 machines.

---

## ⚡ Quick Launch in VS Code
Open this folder in VS Code, open the integrated terminal (`Ctrl + ~`), and run:
```powershell
dotnet run
```
Or build a standalone release executable:
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

---

## 🎯 Complete Legacy MiPOS Feature Parity Matrix

### 1. File & Security
- **Switch Cashier / Cashier Login (`LoginWindow`)**: Cashier & Manager PIN/Password authentication, switching active cashier session dynamically.
- **Lock POS Terminal**: Fast till locking for breaks and shift changes.
- **Role-Based Access Control**: Cashiers, Managers, and Admins with granular permissions (`CanDiscount`, `CanDelete`, `CanPriceChange`, `CanCashUp`).

### 2. Stock & Inventory Management
- **Stock Maintenance (`StockMaintenanceWindow`)**: Full product master catalog with Barcode, Description, Department, Cost Excl, Selling Price Incl, and SOH.
- **Department Maintenance (`DepartmentMaintenanceWindow`)**: Department codes, descriptions, and default markup percentages.
- **Stock Adjustments (`StockAdjustmentWindow`)**: Debit/Credit inventory adjustments for breakage, shrinkage, expiry, and internal transfers with audit logging.
- **Stock Take & Audit (`StockTakeWindow`)**: Physical count reconciliation, system SOH comparison, variance calculation, and one-click batch stock variance posting.
- **Barcode & Shelf Labels (`ShelfLabelWindow`)**: Retail shelf talker generator with barcode, product code, description, and large VAT-inclusive pricing ready for label printers.
- **Zero SOH Report (`ReportsWindow`)**: Instant filter of out-of-stock items requiring supplier re-orders.
- **Stock Valuation Report (`ReportsWindow`)**: Total inventory valuation calculated at both Cost (Excl) and Retail (Incl).

### 3. Creditors & Purchasing
- **Goods Received Note - GRN (`GrnWindow`)**: Receiving stock against supplier invoices, automatic cost recalculation, and stock level incrementing.
- **Creditors Maintenance (`CreditorMaintenanceWindow`)**: Supplier profiles, payment terms (e.g. 30 Days), contact information, and address records.
- **Purchase Orders & Audit**: Supplier purchasing workflow tracking.

### 4. Debtors & Customer Accounts
- **Account Lookup (`DebtorLookupWindow` / `Ctrl+A`)**: Instant customer search with credit limits, available credit, and current balances.
- **Debtors Maintenance (`DebtorMaintenanceWindow`)**: Customer account creation, credit limit terms, and phone/address directory.
- **Customer Laybyes (`LaybyeWindow`)**: Laybye tracking, deposit logging, payment installments, and final item release upon full settlement.
- **Formal Quotations (`QuoteWindow`)**: Save current POS cart as a formal quote, print customer quotation slips, and convert accepted quotes directly back into active POS sales.
- **Statements & Age Analysis (`AgeAnalysisWindow`)**: Real-time debtor aging breakdown into Current, 30 Days, 60 Days, 90 Days, and 120+ Days overdue buckets with statement generation.

### 5. Utility & Shift Management
- **Cash Up Reconciliation (`CashUpWindow`)**: Shift cash up calculating opening float, cash sales, card sales, petty cash payouts, expected till cash vs. counted cash, and variance.
- **Petty Cash (`PettyCashWindow`)**: Expense vouchers with category breakdowns (Tea/Coffee, Cleaning, Repairs, Transport).
- **Day End Process**: Transaction consolidation and audit rolling.
- **Month End Process**: Debtor ledger aging roll-forward.
- **Company & Store Details (`CompanyDetailsWindow`)**: Store name, VAT number, physical address, and custom receipt footer configuration.
- **User & Cashier Maintenance (`UserMaintenanceWindow`)**: Add, edit, and assign security roles to till operators.
- **MySQL Database Configuration (`DatabaseConfigWindow`)**: Live MySQL host, port, database name, username, and password testing with automatic persistence to `config.json`.

### 6. High-Speed POS Register
- **Instant Barcode Scanning**: Sub-millisecond barcode lookup with auto-focus and quantity multiplier (e.g. enter 6 then scan).
- **F3 Stock Search Modal**: Quick search by description or department when barcodes are damaged or unreadable.
- **F5 Line / Cart Discounts**: Quick percentage discounts (5%, 10%, 15%, 20% or custom) with authorization checks.
- **F6 Manager Price Overrides**: Single-item price override modal with immediate recalculation of VAT and line totals.
- **F8 Returns & Credit Notes**: Toggle return mode to ring up customer returns with negative quantities, automatic stock restitution to inventory, and ESC/POS Credit Note slip generation (`CN-XXXXX`).
- **F10 Slip Reprint**: Quick reprint of the last customer receipt.
- **F12 Split / Fast Payment Modal**: Cash, Credit Card, Debit Card, and Account tender with exact change calculation and cash drawer kick command.
- **ESC/POS Thermal 80mm Printing**: Receipts automatically saved as text files under `./Receipts/` and ready for USB/Serial/Network thermal printers.
- **Offline Fallback Resilience**: Seamless fallback to in-memory demo supermarket inventory when MySQL server is unreachable.
