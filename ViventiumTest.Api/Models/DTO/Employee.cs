namespace ViventiumTest.Api.Models.DTO
{
    public class Employee
    {
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public EmployeeHeader[] Managers { get; set; } = [];
    }
}
