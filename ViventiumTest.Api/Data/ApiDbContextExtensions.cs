using Microsoft.EntityFrameworkCore;

namespace ViventiumTest.Api.Data
{
    public partial class ApiDbContext
    {
        public virtual DbSet<ManagerChainResult> ManagerChainResult { get; set; }


        public async Task<List<ManagerChainResult>> ManagerChain(int companyId, string employeeNumber)
        {
            var result = await this.Set<ManagerChainResult>()
                .FromSqlInterpolated($"EXEC ManagerChain {companyId}, {employeeNumber}")
                .ToListAsync();

            return result;
        }
    }
}
