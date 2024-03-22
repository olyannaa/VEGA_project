namespace vega;

public partial class KKSFile
{
    public int Id { get; set; }

    public string? KKSId { get; set; }

    public string FileName { get; set; } = null!;

    public DateTime? UploadDate  { get; set; }
}