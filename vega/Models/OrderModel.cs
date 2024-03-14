using System.ComponentModel.DataAnnotations;

public class OrderModel
{
    [Required]
    public string? OrderKKS { get; set; }
    public string? Description { get; set; }
}