namespace PkMechScheduler.Api.Models;

public class BlockModel
{
    public byte Number { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public byte Blocks { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }
    public bool? EvenWeek { get; set; }
    public string? Initials { get; set; }
    public string? Place { get; set; }
}