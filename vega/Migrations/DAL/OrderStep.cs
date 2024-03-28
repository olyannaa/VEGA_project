namespace vega;

public partial class OrderStep
{
    public int StepId { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }
    
    public bool IsApproved { get; set; }

    public DateTime? UploadDate  { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Step? Step { get; set; }

    public virtual User? User { get; set; }
}