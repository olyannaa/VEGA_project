using System;
using System.Collections.Generic;

namespace vega;

public partial class Area
{
    public int Id { get; set; }

    public string? AreaName { get; set; }

    public virtual ICollection<AreaUser>? AreaUsers { get; }
}
