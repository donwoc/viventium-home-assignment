using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViventiumTest.Api.Data;

namespace ViventiumTest.Api.Controllers
{
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger<CompaniesController> _logger; 
        private readonly ApiDbContext _apiDbContext;

        public CompaniesController(ApiDbContext apiDbContext, ILogger<CompaniesController> logger)
        {
            _apiDbContext = apiDbContext;
            _logger = logger;
        }

        [Route("/companies")]
        [HttpGet]
        public async Task<ObjectResult> GetCompanies()
        {
            try
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

                _logger.LogInformation($"Returning {result.Count} companies.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies");
                return BadRequest(ex.Message);
            }
        }

        [Route("/companies/{companyId}")]
        [HttpGet]
        public async Task<ActionResult<Models.DTO.Company>> GetCompany(int companyId)
        {
            try
            {
                var dbCompany = await _apiDbContext
                    .Company
                    .Include(x => x.Employee)
                    .SingleOrDefaultAsync(x => x.CompanyId == companyId);

                if (dbCompany == null)
                {
                    _logger.LogWarning($"Company id {companyId} not found.");
                    return NotFound();
                }

                var result = new Models.DTO.Company
                {
                    Id = dbCompany.CompanyId,
                    Code = dbCompany.Code,
                    Description = dbCompany.Description,
                    EmployeeCount = dbCompany.Employee.Count(),
                    Employees = dbCompany.Employee.Select(x => new Models.DTO.EmployeeHeader
                    {
                        EmployeeNumber = x.EmployeeNumber,
                        FullName = $"{x.FirstName} {x.LastName}"
                    }).ToArray()
                };

                _logger.LogInformation($"Returning company id {companyId}.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company id {companyId}");  
                return BadRequest(ex.Message);
            }
        }


        [Route("/companies/{companyId}/employees/{employeeNumber}")]
        [HttpGet]
        public async Task<ActionResult<Models.DTO.Employee>> GetCompanyEmployee(int companyId, string employeeNumber)
        {
            try
            {
                var dbEmployee = await _apiDbContext
                    .Employee
                    .Include(x => x.Company)
                    .Include(x => x.Department)
                    .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.EmployeeNumber == employeeNumber);

                if (dbEmployee == null)
                {
                    _logger.LogWarning($"Company id {companyId}, employeeNumber {employeeNumber} not found.");
                    return NotFound();
                }

                var result = new Models.DTO.Employee
                {
                    EmployeeNumber = dbEmployee.EmployeeNumber,
                    FullName = $"{dbEmployee.FirstName} {dbEmployee.LastName}",
                    Email = dbEmployee.Email,
                    Department = dbEmployee.Department.Name,
                    HireDate = dbEmployee.HireDate,
                    Managers = []
                };

                //Get the management chain
                var managers = await _apiDbContext.ManagerChain(companyId, employeeNumber);
                result.Managers = managers
                    .Select(x => new Models.DTO.EmployeeHeader
                    {
                        EmployeeNumber = x.EmployeeNumber,
                        FullName = $"{x.FirstName} {x.LastName}",
                    })
                    .ToArray();

                _logger.LogInformation($"Returning company id {companyId}, employeeNumber {employeeNumber}.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company id {companyId}, employeeNumber {employeeNumber}");
                return BadRequest(ex.Message);
            }
        }
    }
}
