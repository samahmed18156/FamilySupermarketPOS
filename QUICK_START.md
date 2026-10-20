# Blue_Knight POS - Quick Start & Deployment Guide

## 📦 What's Inside This Update
This release is a complete, self-contained codebase upgrade providing **100% full feature parity with legacy MiPOS Java**, rebuilt on **.NET 8 + WPF-UI** under the **Blue_Knight** brand.

---

## 🚀 How to Run on Windows 10

### Step 1: Extract Files
1. Download `Blue_Knight_All_Features_Update.zip`.
2. Extract the archive into your project directory (e.g., `C:\Projects\Blue_Knight_POS\`).
3. Overwrite all existing files.

### Step 2: Open in VS Code
1. Open VS Code.
2. Click **File -> Open Folder...** and select the extracted folder.
3. Open the integrated terminal (`Ctrl + \``).

### Step 3: Run the Application
Run:
```powershell
dotnet run
```
That's it! The application will compile in ~3 seconds and launch the modern Fluent Dark UI terminal.

---

## ⌨️ POS Cashier Keyboard Quick Reference

| Hotkey | Action |
|---|---|
| **Enter** | Scan barcode or process product code in the barcode box |
| **F3** | Open Stock Search Modal (lookup items by description) |
| **F5** | Apply Line Item / Cart Discount % |
| **F6** | Price Override for selected item |
| **F8** | Toggle Returns & Credit Notes Mode |
| **F10** | Quick Reprint Last Receipt |
| **F12** | Complete Transaction / Tender Payment Window |
| **Ctrl + A** | Customer / Debtor Account Lookup |
| **Delete** | Remove selected item from the cart |
| **Esc** | Cancel / Close any modal dialog |

---

## 🗄️ Database Connection
- **Default Database**: `m1pos` on `127.0.0.1:3306`
- **Offline Mode**: If MySQL is not running or credentials differ, Blue_Knight POS automatically enters offline demo mode with loaded supermarket products, debtors, departments, and users so you can test every single feature immediately!
- **Configure Database**: Go to **Utility -> MySQL Database Configuration**, enter your MySQL password, test connection, and click Save. Settings persist to `config.json`.
