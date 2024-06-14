namespace vega;

public partial class Order
{
    public int Id { get; set; }

    public string KKS { get; set; } = null!;

    public virtual ICollection<OrderFile> OrderFiles { get; set; } = null!;

    public virtual ICollection<OrderStep> OrderSteps { get; set; } = null!;

    public virtual ICollection<StorageComponent>? Storage {get; set;}
}