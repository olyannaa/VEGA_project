

public partial class Designation
{
    public int Id { get; set; }

    public int FullName { get; set; }

    public int ProcessId { get; set; }

    public int SchemesId { get; set; }

    public TechProccess Proccess { get; set; } = null!;

    public Scheme Scheme { get; set; } = null!;

    public Component Component{ get; set; } = null!;
}