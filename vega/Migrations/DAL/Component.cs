
public partial class Component
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public int DesignationId { get; set; }

    public int Amount { get; set; }

    public int Count { get; set; }

    public bool IsDeveloped { get; set;}

     public Designation Designation { get; set; } = null!;

    public OrderComponent OrderComponent { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = null!;

    public virtual Component? Parent { get; set; }

    public virtual Component? Child { get; set; }
}