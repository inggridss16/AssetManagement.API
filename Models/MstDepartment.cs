using System;
using System.Collections.Generic;

namespace AssetManagement.API.Models;

public partial class MstDepartment
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public long? UpdatedBy { get; set; }
}
