using System.ComponentModel.DataAnnotations;

public class UserAuthModel
{
    [Required]
    public string? Login {get; set;}

    [Required]
    public string? Password {get; set;}
}