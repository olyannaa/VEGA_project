using System;
using System.Collections.Generic;

namespace vega;

public partial class Role
{
    public int Id { get; set; }

    public string? Role1 { get; set; }

    public virtual ICollection<RoleUser> RoleUsers { get; } = new List<RoleUser>();
}
