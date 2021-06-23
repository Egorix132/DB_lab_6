using DB_lab_6.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TeacherRow = DB_lab_6.Models.TeacherRow;

namespace DB_lab_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string connectionString;
        private readonly SqlConnection connection;
        private readonly DbContext context;
        public MainWindow()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            TeachersGrid.SelectedCellsChanged += TeacherRowSelected;
            DepartmentGrid.SelectedCellsChanged += DepartmentRowSelected;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                context = new DbContext(connection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTables();
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersGrid.SelectedItems != null)
            {
                for (int i = 0; i < TeachersGrid.SelectedItems.Count; i++)
                {
                    if (TeachersGrid.SelectedItems[i] is TeacherRow item)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Name))
                        {
                            context.DeleteEntity("Teachers", new Dictionary<string, object> { { "id", item.TeacherId } });
                        }
                    }
                }
            }

            if (DepartmentGrid.SelectedItems != null)
            {
                for (int i = 0; i < DepartmentGrid.SelectedItems.Count; i++)
                {
                    if (DepartmentGrid.SelectedItems[i] is DepartmentRow item)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Name))
                        {
                            context.DeleteEntity("Departments", new Dictionary<string, object> { { "id", item.DepartmentId } });
                        }
                    }
                }
            }

            LoadTables();
        }

        private void Update()
        {
            TeacherRow teacherRow = (TeacherRow)TeachersGrid.SelectedItem;
            if(teacherRow == null)
            {
                DepartmentRow departmentRow = (DepartmentRow)DepartmentGrid.SelectedItem;
                context.UpdateDepartmentRow(departmentRow);
            }
            else
            {
                context.UpdateTeacherRow(teacherRow);
            }

            LoadTables();
        }

        private void LoadTables()
        {
            TeachersGrid.ItemsSource = context.GetTeachers();
            DepartmentGrid.ItemsSource = context.GetDepartments();
        }

        private void TeacherRowSelected(object sender, SelectedCellsChangedEventArgs e)
        {
            DepartmentGrid.UnselectAll();
        }

        private void DepartmentRowSelected(object sender, SelectedCellsChangedEventArgs e)
        {
            TeachersGrid.UnselectAll();
        }
    }
}
