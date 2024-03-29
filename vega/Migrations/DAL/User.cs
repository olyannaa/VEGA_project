using System;
using System.Collections.Generic;

namespace vega;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public string? FullName { get; set;}

    public virtual AreaUser? AreasUser { get; set; }

    public virtual ICollection<RoleUser> RoleUsers { get; } = null!;

    public virtual ICollection<OrderStep> OrderSteps { get; set; } = null!;
}
