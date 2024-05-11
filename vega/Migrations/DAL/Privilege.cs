public class Privilege
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<RolePrivilege> Roles { get; } = null!;
}