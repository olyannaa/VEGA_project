using System.ComponentModel.DataAnnotations;

public class RefreshModel
{
    [Required]
    public string? RefreshToken {get; set;}
}