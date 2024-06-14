using vega;

public partial class Task
{
    public int Id { get; set; }

    public int ComponentId { get; set; }

    public int? UserId { get; set; }

    public int AreaId { get; set; }

    public int StatusId { get; set;}

    public int ParentId { get; set;}

    public bool IsAvaliable { get; set;}

    public Area Area { get; set; } = null!;

    public Component Component { get; set; } = null!;

    public Status Status { get; set; } = null!;

    public User? User { get; set; }

    public virtual Task? Parent { get; set; }

    public virtual Task? Child { get; set; }
}
