

public partial class Designation
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;
 
    public int ProcessId { get; set; }

    public int SchemesId { get; set; }

    public TechProcess Proccess { get; set; } = null!;

    public Scheme Scheme { get; set; } = null!;

    public Component Component{ get; set; } = null!;
}