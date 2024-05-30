using System;
using System.Collections.Generic;

namespace ViventiumTest.Api.Data;

public partial class Employee
{
    public string EmployeeNumber { get; set; } = null!;

    public int CompanyId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? HireDate { get; set; }

    public int DepartmentId { get; set; }

    public string? ManagerEmployeeNumber { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;

    public virtual Employee? EmployeeNavigation { get; set; }

    public virtual ICollection<Employee> InverseEmployeeNavigation { get; set; } = new List<Employee>();
}
