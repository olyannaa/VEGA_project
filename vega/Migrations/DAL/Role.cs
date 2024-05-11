using System;
using System.Collections.Generic;

namespace vega;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RoleUser> RoleUsers { get; } = null!;

    public virtual ICollection<RolePrivilege> RolePrivileges { get; } = null!;

    public virtual StepRole StepRole { get; } = null!;
}
