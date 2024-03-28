namespace vega;

public partial class Order
{
    public int Id { get; set; }

    public string? KKS { get; set; } = null!;

    public virtual OrderFile? OrderFile { get; set; }

    public virtual ICollection<OrderStep> OrderSteps { get; set; } = new List<OrderStep>();
}