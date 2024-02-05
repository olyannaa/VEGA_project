using System.ComponentModel.DataAnnotations;

public class UserCreationModel
{
    [Required]
    public string? Login {get; set;}

    [Required]
    public string? Password {get; set;}

    [Required]
    public string? Name {get; set;}

    [Required]
    public int? RoleId {get; set;}

    [Required]
    public int? AreaId {get; set;}
}