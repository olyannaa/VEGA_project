using vega;

public class RolePrivilege
{
    public int Id { get; set;}

    public int RoleId { get; set;}

    public int PrivilegeId { get; set; }

    public Role Role { get; set;} = null!;

    public Privilege Privilege { get; set; } = null!;
}