using System.ComponentModel.DataAnnotations;

public class UpdateTaskModel
{
    [Required]
    public int TaskId {get; set;}

    [Required]
    public int StatusId {get; set;}

}