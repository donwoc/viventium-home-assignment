using Microsoft.EntityFrameworkCore;

namespace ViventiumTest.Api.Data
{
    [Keyless]
    public class ManagerChainResult
    {
        public int Level { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public  string FirstName { get; set; } = string.Empty;
        public  string LastName { get; set; } = string.Empty;
    }
}
