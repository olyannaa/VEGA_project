public partial class Scheme
{
    public int Id { get; set; }

    public string? Path { get; set; }

    public Designation Designation { get; set; } = null!;
}