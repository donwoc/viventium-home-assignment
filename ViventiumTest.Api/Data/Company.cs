using System;
using System.Collections.Generic;

namespace ViventiumTest.Api.Data;

public partial class Company
{
    public int CompanyId { get; set; }

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Department> Department { get; set; } = new List<Department>();

    public virtual ICollection<Employee> Employee { get; set; } = new List<Employee>();
}
