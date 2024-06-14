
using vega;

public partial class OrderComponent
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ComponentId { get; set; }

    public Order Order { get; set;} = null!;

    public Component? Component { get; set; }
}