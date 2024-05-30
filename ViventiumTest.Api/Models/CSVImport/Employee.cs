namespace ViventiumTest.Api.Models.CSVImport
{
    public class Employee
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public string ManagerEmployeeNumber { get; set; } = string.Empty;
    }
}
