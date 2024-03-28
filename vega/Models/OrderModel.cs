using System.ComponentModel.DataAnnotations;

public class OrderModel
{
    [Required]
    public string? KKS { get; set; }
    public string? Description { get; set; }
}