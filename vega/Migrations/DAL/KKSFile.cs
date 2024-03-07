namespace vega;

public partial class KKSFile
{
    public int Id { get; set; }

    public string? KKSId { get; set; }

    public string? FileName { get; set; }

    public DateTime? UploadDate  { get; set; }

    public bool Status { get; set; }
}