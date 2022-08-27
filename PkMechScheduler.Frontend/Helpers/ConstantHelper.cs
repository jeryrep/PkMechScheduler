namespace PkMechScheduler.Frontend.Helpers;

public static class ConstantHelper
{
    public static IReadOnlyCollection<string> Hours { get; } = new[]
    {
        "7:30-8:15", "8:15-9:00", "9:15-10:00", "10:00-10:45", "11:00-11:45", "11:45-12:30", "12:45-13:30",
        "13:30-14:15", "14:30-15:15", "15:15-16:00", "16:15-17:00", "17:00-17:45", "18:00-18:45", "18:45-19:30",
        "19:45-20:30", "20:30-21:15"
    };
}