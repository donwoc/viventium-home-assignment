using LINQtoCSV;
using Microsoft.EntityFrameworkCore;

namespace ViventiumTest.Api.Lib
{
    public class CSVImporter
    {
        public async Task<Models.CSVImport.Result> ImportFileAsync(string filePath, Data.ApiDbContext apiDbContext)
        {
            try
            {
                //Load the data from the CSV file
                var inputRows = LoadData(filePath);

                //Parse the data in to a list of Company objects, and a list of errors
                var companies = ParseData(inputRows, out List<string> errors);

                //If there are any errors, don't import the data, return the error list
                if (errors.Count != 0)
                {
                    return new Models.CSVImport.Result
                    {
                        Success = false,
                        Errors = errors
                    };
                }

                //Add data to the database
                await SaveDataAsync(companies, apiDbContext);

                //Everything was successful
                return new Models.CSVImport.Result
                {
                    Success = true,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                return new Models.CSVImport.Result
                {
                    Success = false,
                    Errors = [$"Unexpected internal error: {ex.Message}"]
                };
            }
        }

        //Load the data from the CSV file in to a list of InputRow objects
        private IEnumerable<Models.CSVImport.InputRow> LoadData(string filePath)
        {
            var inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                IgnoreUnknownColumns = false
            };

            var cc = new CsvContext();
            var inputRows = cc.Read<Models.CSVImport.InputRow>(filePath, inputFileDescription);

            return inputRows;
        }

        //Parse the data in to a list of Company objects, and a list of errors
        private List<Models.CSVImport.Company> ParseData(IEnumerable<Models.CSVImport.InputRow> inputRows, out List<string> errors)
        {
            errors = [];

            //Get a list of companies
            var companies = inputRows
                .GroupBy(ir => ir.CompanyId)
                .Select(group => new Models.CSVImport.Company
                {
                    CompanyId = group.Key,
                    Code = group.First().CompanyCode,
                    Description = group.First().CompanyDescription
                })
                .ToList();

            //Get a list of departments for each company
            int departmentId = 0;
            foreach (var company in companies)
            {
                //We get a distinct list of departments for the company, and assign a unique department ID to each
                var inputDepartment = inputRows
                    .Where(ir => ir.CompanyId == company.CompanyId)
                    .Select(ir => ir.EmployeeDepartment)
                    .Distinct()
                    .ToList();

                company.Departments = [];

                foreach (var department in inputDepartment)
                {
                    if (!company.Departments.Any(x => x.Name == department))
                    {
                        company.Departments.Add(new Models.CSVImport.Department
                        {
                            DepartmentId = ++departmentId,
                            Name = department
                        });
                    }
                }
            }

            //Get a list of employees for each company
            foreach (var company in companies)
            {
                var employees = inputRows
                    .Where(ir => ir.CompanyId == company.CompanyId)
                    .Select(ir => new Models.CSVImport.Employee
                    {
                        EmployeeNumber = ir.EmployeeNumber,
                        FirstName = ir.EmployeeFirstName,
                        LastName = ir.EmployeeLastName,
                        Email = ir.EmployeeEmail,
                        DepartmentId = company.Departments.Single(x => x.Name == ir.EmployeeDepartment).DepartmentId, //Get the department ID from the department name
                        HireDate = ir.HireDate,
                        ManagerEmployeeNumber = ir.ManagerEmployeeNumber
                    });


                //Add employees to the company only if it has a unique employee number
                foreach (var employee in employees)
                {
                    //Validate that the employee number is unique for the company
                    if (company.Employees.Any(x => x.EmployeeNumber == employee.EmployeeNumber))
                    {
                        errors.Add($"Employee number {employee.EmployeeNumber} is not unique for company {company.Code}.");
                    }
                    else
                    {
                        //Validate that the manager employee number exists in the company
                        if (!string.IsNullOrEmpty(employee.ManagerEmployeeNumber) && !employees.Any(x => x.EmployeeNumber == employee.ManagerEmployeeNumber))
                        {
                            errors.Add($"Manager employee number {employee.ManagerEmployeeNumber} for employee {employee.EmployeeNumber} does not exist in company {company.Code}.");
                        }
                        else //Employee record is valid
                        {
                            company.Employees.Add(employee);
                        }
                    }
                }
            }

            return companies;
        }

        private async Task SaveDataAsync(List<Models.CSVImport.Company> companies, Data.ApiDbContext apiDbContext)
        {
            //Wrap it all in a transaction to avoid partial data being saved
            using var transaction = apiDbContext.Database.BeginTransaction();

            try
            {
                //clean the database
                await apiDbContext.Database.ExecuteSqlRawAsync("delete from Employee");
                await apiDbContext.Database.ExecuteSqlRawAsync("delete from Department");
                await apiDbContext.Database.ExecuteSqlRawAsync("delete from Company");

                var dbEmployees = new List<Data.Employee>();

                //Add companies to the database
                foreach (var company in companies)
                {
                    var dbCompany = new Data.Company
                    {
                        CompanyId = company.CompanyId,
                        Code = company.Code,
                        Description = company.Description
                    };

                    await apiDbContext.Company.AddAsync(dbCompany);

                    //add departments to the company
                    foreach (var department in company.Departments)
                    {
                        var dbDepartment = new Data.Department
                        {
                            DepartmentId = department.DepartmentId,
                            Name = department.Name,
                            CompanyId = company.CompanyId
                        };

                        apiDbContext.Department.Add(dbDepartment);

                        //add employees to the department
                        var employees = company.Employees.Where(x => x.DepartmentId == department.DepartmentId).ToList();
                        foreach (var employee in employees)
                        {
                            var dbEmployee = new Data.Employee
                            {
                                EmployeeNumber = employee.EmployeeNumber,
                                CompanyId = company.CompanyId,
                                DepartmentId = department.DepartmentId,
                                FirstName = employee.FirstName,
                                LastName = employee.LastName,
                                Email = employee.Email,
                                HireDate = employee.HireDate,
                                ManagerEmployeeNumber = null, //We will update this later to avoid check of foreign key constraints and circular references
                            };

                            dbEmployees.Add(dbEmployee); //Keep a list of employees to update the manager employee number later
                        }
                    }
                }

                apiDbContext.Employee.AddRange(dbEmployees);

                await apiDbContext.SaveChangesAsync();

                //Update the manager employee number to the correct foreign key
                foreach (var dbEmployee in dbEmployees)
                {
                    var managerEmployeeNumber = companies
                        .Single(x => x.CompanyId == dbEmployee.CompanyId)
                        .Employees
                        .SingleOrDefault(x => x.EmployeeNumber == dbEmployee.EmployeeNumber)?
                        .ManagerEmployeeNumber;
                    
                    if (!String.IsNullOrEmpty(managerEmployeeNumber))
                        dbEmployee.ManagerEmployeeNumber = managerEmployeeNumber;
                }

                await apiDbContext.SaveChangesAsync();

                //Finally, commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error saving data to the database: {ex.Message}");
            }
        }
    }
}
