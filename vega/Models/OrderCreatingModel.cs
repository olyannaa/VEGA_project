using System.ComponentModel.DataAnnotations;

public class OrderCreatingModel
{
    [Required]
    public string? OrderKKS {get; set;}
    [Required]
    public string? Description {get; set;}
}