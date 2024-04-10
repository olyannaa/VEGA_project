using System.ComponentModel.DataAnnotations;

public class StatisticsModel
{
    public int Total { get; set; }

    public int OnEntry { get; set;}

    public int OnTIDev { get; set; }

    public int OnDDDev { get; set; }

    public int OnIDPPSDev { get; set; }

    public int OnApproval { get; set; }

    public int OnSupply { get; set; }

    public int OnStorage { get; set;}

    public int Completed { get; set; }
}