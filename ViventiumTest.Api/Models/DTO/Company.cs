namespace ViventiumTest.Api.Models.DTO
{
    public class Company : CompanyHeader
    {
        public EmployeeHeader[] Employees { get; set; } = [];
    }
}
