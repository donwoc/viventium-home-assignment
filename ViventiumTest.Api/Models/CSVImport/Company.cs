namespace ViventiumTest.Api.Models.CSVImport
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<Department> Departments { get; set; } = [];

        public List<Employee> Employees { get; set; } = [];
    }
}
