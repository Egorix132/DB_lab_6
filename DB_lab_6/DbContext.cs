using DB_lab_6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DB_lab_6
{
    public class DbContext
    {
        private readonly SqlConnection connection;
        public DbContext(SqlConnection connection)
        {
            this.connection = connection;
        }

        public List<TeacherRow> GetTeachers()
        {
            string sql = "SELECT t.id as TeacherId, Name, Experience, d.id as DepartmentId, Title FROM Teachers as t LEFT JOIN Departments AS d ON DepartmentId = d.id";

            SqlCommand command = new(sql, connection);
            SqlDataAdapter adapter = new(command);

            DataTable table = new();
            adapter.Fill(table);

            List<TeacherRow> tableList = new List<TeacherRow>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                TeacherRow row = new TeacherRow();
                row.TeacherId = Convert.ToInt32(table.Rows[i]["TeacherId"]);
                row.Experience
                    = Convert.IsDBNull(table.Rows[i]["Experience"]) ? null : Convert.ToInt32(table.Rows[i]["Experience"]);
                row.Name = table.Rows[i]["Name"].ToString();

                row.DepartmentId
                    = Convert.IsDBNull(table.Rows[i]["DepartmentId"]) ? null : Convert.ToInt32(table.Rows[i]["DepartmentId"]);
                row.Title = table.Rows[i]["Title"].ToString();
                tableList.Add(row);
            }

            return tableList.OrderBy(t => t.TeacherId).ThenBy(t => t.DepartmentId).ToList();
        }

        public List<DepartmentRow> GetDepartments()
        {
            string sql = "SELECT d.id as DepartmentId, Title, f.id as FacultyId, Name FROM Departments as d LEFT JOIN Faculties AS f ON FacultyId = f.id";

            SqlCommand command = new(sql, connection);
            SqlDataAdapter adapter = new(command);

            DataTable table = new();
            adapter.Fill(table);

            List<DepartmentRow> tableList = new List<DepartmentRow>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DepartmentRow row = new DepartmentRow();
                row.DepartmentId = Convert.ToInt32(table.Rows[i]["DepartmentId"]);
                row.Title = table.Rows[i]["Title"].ToString();
                row.FacultyId
                    = Convert.IsDBNull(table.Rows[i]["FacultyId"]) ? null : Convert.ToInt32(table.Rows[i]["FacultyId"]);
                row.Name = table.Rows[i]["Name"].ToString();
                tableList.Add(row);
            }

            return tableList.OrderBy(t => t.DepartmentId).ToList();
        }

        public string GetEntityId(string table, Dictionary<string, object> filter)
        {
            string sql = $"SELECT id FROM {table} " +
                $"WHERE {string.Join(" AND ", filter.Select(f => $"\"{f.Key}\" = {(f.Value is string ? $"\'{f.Value}\'" : f.Value)}"))}";

            SqlDataReader rdr = null;
            try
            {
                SqlCommand command = new(sql, connection);
                rdr = command.ExecuteReader();
                rdr.Read();
                string res = rdr["id"].ToString();
                rdr.Close();
                return res;
            }
            catch (Exception)
            {
                if (rdr != null && !rdr.IsClosed)
                {
                    rdr.Close();
                }
                return null;
            }
        }

        public void InsertEntity(string table, Dictionary<string, object> values)
        {
            string sql = $"INSERT INTO {table} " +
                $"({string.Join(", ", values.Keys)}) " +
                $"VALUES ({string.Join(", ", values.Values.Select(v => v is null ? "NULL" : v is string ? $"\'{v}\'" : v))})";

            SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }

        public void UpdateEntity(string table, Dictionary<string, object> values, Dictionary<string, object> filter)
        {
            string sql = $"UPDATE {table} " +
                $"SET {string.Join(", ", values.Select(f => $"\"{f.Key}\" = {(f.Value is null ? "NULL" : f.Value is string ? $"\'{f.Value}\'" : f.Value)}"))} " +
                $"WHERE {string.Join(" AND ", filter.Select(f => $"\"{f.Key}\" = {(f.Value is string ? $"\'{f.Value}\'" : f.Value)}"))}";

            SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }

        public void DeleteEntity(string table, Dictionary<string, object> filter)
        {
            string sql = $"DELETE FROM {table} " +
                $"WHERE {string.Join(" AND ", filter.Select(f => $"\"{f.Key}\" = {(f.Value is string ? $"\'{f.Value}\'" : f.Value)}"))}";

            SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }

        public void UpdateTeacherRow(TeacherRow row)
        {
            if (row == null)
            {
                return;
            }

            string departmentId = null;
            if (!string.IsNullOrWhiteSpace(row.Title))
            {
                departmentId = GetEntityId("Departments", new Dictionary<string, object> { { "Title", row.Title } });
            }

            if (!string.IsNullOrWhiteSpace(row.Name))
            {
                string teacherId = GetEntityId("Teachers", new Dictionary<string, object> { { "id", row.TeacherId } });
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    InsertEntity("Teachers", new Dictionary<string, object> {
                        { "Name", row.Name },
                        { "Experience", row.Experience },
                        { "DepartmentId", departmentId },
                    });
                }
                else
                {
                    UpdateEntity("Teachers", new Dictionary<string, object> {
                        { "Name", row.Name },
                        { "Experience", row.Experience },
                        { "DepartmentId", departmentId },
                    },
                    new Dictionary<string, object> { { "id", teacherId.ToString() } });
                }
            }
        }

        public void UpdateDepartmentRow(DepartmentRow row)
        {
            if(row == null)
            {
                return;
            }
            string facultyId = null;
            if (!string.IsNullOrWhiteSpace(row.Name))
            {
                facultyId = GetEntityId("Faculties", new Dictionary<string, object> { { "Name", row.Name } });
            }

            if (!string.IsNullOrWhiteSpace(row.Title))
            {
                string departmentId = GetEntityId("Departments", new Dictionary<string, object> { { "id", row.DepartmentId } });
                if (string.IsNullOrWhiteSpace(departmentId))
                {
                    InsertEntity("Departments",
                        new Dictionary<string, object> { { "Title", row.Title }, { "FacultyId", facultyId } });
                }
                else
                {
                    UpdateEntity("Departments",
                        new Dictionary<string, object> { { "Title", row.Title }, { "FacultyId", facultyId } },
                        new Dictionary<string, object> { { "id", departmentId.ToString() } });
                }
            }
        }
    }
}
