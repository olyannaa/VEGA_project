namespace vega;

public partial class OrderFile
{
    public int FileId { get; set; }

    public int OrderId { get; set; }

    public string FileName { get; set; } = null!;

    public bool IsNeededToChange { get; set; }

    public DateTime? UploadDate  { get; set; }

    public virtual Order? Area { get; set; }
}