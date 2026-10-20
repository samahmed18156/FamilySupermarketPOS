using System.Collections.ObjectModel;
using System.Windows;
using FamilySupermarketPOS.Models;
using FamilySupermarketPOS.Services;
using Wpf.Ui.Controls;

namespace FamilySupermarketPOS.Views
{
    public partial class DepartmentMaintenanceWindow : FluentWindow
    {
        public ObservableCollection<Department> Departments { get; set; } = new();

        public DepartmentMaintenanceWindow()
        {
            InitializeComponent();
            GridDepts.ItemsSource = Departments;
            Reload();
        }

        private void Reload()
        {
            Departments.Clear();
            foreach (var d in DatabaseService.GetDepartments()) Departments.Add(d);
            if (Departments.Count > 0) GridDepts.SelectedIndex = 0;
        }

        private void GridDepts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridDepts.SelectedItem is Department selected)
            {
                TxtCode.Text = selected.Code;
                TxtName.Text = selected.Name;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtCode.Text.Trim().ToUpperInvariant();
            string name = TxtName.Text.Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            {
                System.Windows.MessageBox.Show("Please enter both Department Code and Name.", "Validation Error");
                return;
            }

            DatabaseService.SaveDepartment(new Department { Code = code, Name = name });
            Reload();
            System.Windows.MessageBox.Show($"Department '{name}' saved successfully!", "Saved");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridDepts.SelectedItem is Department selected)
            {
                DatabaseService.DeleteDepartment(selected.Code);
                Reload();
            }
        }
    }
}
