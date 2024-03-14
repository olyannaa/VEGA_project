using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;

public class UserCreationModel
{
    [Required]
    public string? Login {get; set;}

    [Required]
    public string? Password {get; set;}

    [Required]
    public string? Name {get; set;}

    [Required]
    public int[]? RoleIds {get; set;}

    [Required]
    public int? AreaId {get; set;}
}