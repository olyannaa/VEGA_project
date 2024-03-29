using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;

public class UserCreationModel
{
    [Required]
    public string Login {get; set;} = null!;

    [Required]
    public string Password {get; set;} = null!;

    [Required]
    public string Name {get; set;} = null!;

    [Required]
    public int[] RoleIds {get; set;} = null!;

    public int? AreaId {get; set;}
}