public partial class TechProccess
{
    public int Id { get; set; }

    public string? Proccess { get; set; }

    public virtual ICollection<Designation>? Designations { get; set; }

}
