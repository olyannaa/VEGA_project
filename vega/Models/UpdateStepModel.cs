using System.ComponentModel.DataAnnotations;

public class UpdateStepModel
{
    [Required]
    public string KKS {get; set;} = null!;

    [Required]
    public int StepId {get; set;}

    public string? Description { get; set; }

    public bool? IsApproved { get; set;}
}