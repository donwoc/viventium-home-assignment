namespace ViventiumTest.Api.Models.DTO
{
    public class CompanyHeader
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int  EmployeeCount { get; set; }
    }
}
