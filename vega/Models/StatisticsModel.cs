using System.ComponentModel.DataAnnotations;

public class StatisticsModel
{
    public int Total { get; set; }

    public int InCompleted { get; set; }

    public int OnApproval { get; set; }

    public int Completed { get; set; }
}