public partial class TechProcess
{
    public int Id { get; set; }

    public string? Process { get; set; }

    public virtual ICollection<Designation>? Designations { get; set; }

    public int[] AreaIds
    {
        get
        {
            if (Process == null)
            {
                return new int[0];
            }
            return Process.Split('-').Select(e => Int32.Parse(e)).ToArray(); 
        }
    } 

}
