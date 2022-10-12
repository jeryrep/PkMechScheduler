namespace PkMechScheduler.Database.Models;

public abstract class BaseBlock
{
    public ulong Id { get; set; }
    public byte Number { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public byte Blocks { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }
    public bool? EvenWeek { get; set; }
    public string? Place { get; set; }
}