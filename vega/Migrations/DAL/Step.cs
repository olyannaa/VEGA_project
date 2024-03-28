namespace vega;

public partial class Step
{
    public int Id { get; set; }

    public string? Name { get; set; } = null!;

    public virtual ICollection<OrderStep> OrderSteps { get; set; } = new List<OrderStep>();
}