namespace vega;

public partial class OrderStep
{
    public int Id { get; set; }
    
    public int StepId { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int? ParentId {get; set;}

    public bool IsCompleted { get; set; }

    public string? Comment { get; set;}

    public virtual Order Order { get; set; } = null!;

    public virtual Step Step { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual OrderStep? Parent { get; set; }

    public virtual ICollection<OrderStep>? Children { get; set; }
}