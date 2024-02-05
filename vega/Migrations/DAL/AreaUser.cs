using System;
using System.Collections.Generic;

namespace vega;

public partial class AreaUser
{
    public int UserId { get; set; }

    public int? AreaId { get; set; }

    public virtual Area? Area { get; set; }

    public virtual User User { get; set; } = null!;
}
