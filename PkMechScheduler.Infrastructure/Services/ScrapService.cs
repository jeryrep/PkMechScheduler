using AngleSharp;
using AngleSharp.Dom;
using PkMechScheduler.Infrastructure.Interfaces;

namespace PkMechScheduler.Infrastructure.Services;

public class ScrapService : IScrapService
{
    private readonly IConfiguration _configuration = Configuration.Default.WithDefaultLoader();
    private const string ListAddress = "https://podzial.mech.pk.edu.pl/stacjonarne/html/lista.html";

    public Task<IDocument> ScrapGroupsTeachersRoomsInfo() => BrowsingContext.New(_configuration).OpenAsync(ListAddress);

    public Task<IDocument> ScrapSchedule(string linkEnd) => BrowsingContext.New(_configuration).OpenAsync($"https://podzial.mech.pk.edu.pl/stacjonarne/html/plany/{linkEnd}");
    public IEnumerable<IDocument> ScrapSchedules(IEnumerable<string> linkEnds) => linkEnds.Select(linkEnd => ScrapSchedule(linkEnd).Result);
}