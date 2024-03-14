using System;
using System.Collections.Generic;

namespace vega;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }
    
    public string? FullName { get; set;}

    public virtual AreaUser? AreasUser { get; set; }

    public virtual ICollection<RoleUser> RoleUsers { get; } = new List<RoleUser>();
}
