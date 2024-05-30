using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViventiumTest.Api.Data;

namespace ViventiumTest.Api.Controllers
{
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ApiDbContext _apiDbContext;

        public CompaniesController(ApiDbContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }

        [Route("/companies")]
        [HttpGet]
        public async Task<IEnumerable<Models.DTO.CompanyHeader>> GetCompanies()
        {
            var result = await _apiDbContext
                .Company
                .Select(x => new Models.DTO.CompanyHeader
                {
                    Id = x.CompanyId,
                    Code = x.Code,
                    Description = x.Description,
                    EmployeeCount = x.Employee.Count()
                })
                .ToListAsync();


            return result;
        }
    }
}
