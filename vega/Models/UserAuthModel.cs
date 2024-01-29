using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class UserAuthModel
{
    [Required]
    public string? Login {get; set;}

    [Required]
    public string? Password {get; set;}
}