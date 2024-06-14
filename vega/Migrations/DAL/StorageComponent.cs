using vega;

public partial class StorageComponent
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? Designation { get; set; }

    public string? Name { get; set; }

    public int Amount { get; set; }

    public int? Count { get; set; }

    public string? Measure { get; set; }

    public string? Material { get; set; }

    public string? ObjectType { get; set; }

    public virtual Order Order {get; set;} = null!;
}