using LINQtoCSV;

namespace ViventiumTest.Api.Lib
{
    public class CSVImporter
    {
        public Models.CSVImport.Result ImportFile(string filePath)
        {
            try
            {
                var inputRows = LoadData(filePath);
                var companies = ParseData(inputRows, out List<string> errors);

                //If there are any errors, don't import the data, return the error list
                if (errors.Count != 0)
                {
                    return new Models.CSVImport.Result
                    {
                        Success = false,
                        Companies = companies,
                        Errors = errors
                    };
                }

                //Add data to the database



                return new Models.CSVImport.Result
                {
                    Success = true,
                    Companies = companies,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                return new Models.CSVImport.Result
                {
                    Success = false,
                    Companies = [],
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
                        Department = ir.EmployeeDepartment,
                        HireDate = ir.HireDate,
                        ManagerEmployeeNumber = ir.ManagerEmployeeNumber
                    });


                //Add employees to the only if it has a unique employee number
                company.Employees = [];

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
    }
}
