namespace vega;

public partial class StepRole
{
    public int Id { get; set; }

    public int StepId { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Step Step { get; set; } = null!;
}
