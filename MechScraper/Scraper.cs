using AngleSharp;
using AngleSharp.Dom;

namespace MechScraper;

public static class Scraper
{
    private static readonly IConfiguration Configuration = AngleSharp.Configuration.Default.WithDefaultLoader();
    private const string ListAddress = "https://podzial.mech.pk.edu.pl/stacjonarne/html/lista.html";

    public static Task<IDocument> ScrapGroupsTeachersRoomsInfo() => BrowsingContext.New(Configuration).OpenAsync(ListAddress);

    public static Task<IDocument> ScrapSchedule(string linkEnd) => BrowsingContext.New(Configuration).OpenAsync($"https://podzial.mech.pk.edu.pl/stacjonarne/html/plany/{linkEnd}");
    public static IEnumerable<IDocument> ScrapSchedules(IEnumerable<string> linkEnds) => linkEnds.Select(linkEnd => ScrapSchedule(linkEnd).Result);
}