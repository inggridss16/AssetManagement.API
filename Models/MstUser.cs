using System;
using System.Collections.Generic;

namespace AssetManagement.API.Models;

public partial class MstUser
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[]? Password { get; set; }

    public int DepartmentId { get; set; }

    public string Title { get; set; } = null!;

    public long SupervisorId { get; set; }

    public bool IsAssetManager { get; set; }

    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public long? UpdatedBy { get; set; }
}
