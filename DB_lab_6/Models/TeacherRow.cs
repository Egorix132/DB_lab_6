namespace DB_lab_6.Models
{
    public class TeacherRow
    {
        public int TeacherId { get; set; }

        public string Name { get; set; }

        public int? Experience { get; set; }

        public int? DepartmentId { get; set; }

        public string Title { get; set; }

        public int FacultyId { get; set; }
    }
}
