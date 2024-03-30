using Minio.DataModel.Args;

namespace vega;

public partial class OrderFile
{
    public int FileId { get; set; }

    public int OrderId { get; set; }

    public int StepId { get; set; }

    public string Path { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public bool IsNeededToChange { get; set; }

    public DateTime UploadDate { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Step? Step { get; set; }
}