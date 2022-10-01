namespace PkMechScheduler.Database.Models;

public class Teacher
{
    public ushort Id { get; set; }
    public string? Name { get; set; }
    public string? Link { get; set; }
    public bool EvenWeek { get; set; }
}