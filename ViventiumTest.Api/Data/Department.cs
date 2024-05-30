using System;
using System.Collections.Generic;

namespace ViventiumTest.Api.Data;

public partial class Department
{
    public int DepartmentId { get; set; }

    public int CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Employee> Employee { get; set; } = new List<Employee>();
}
