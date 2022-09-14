using AngleSharp;
using AngleSharp.Dom;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Services;

public class ScrapService : IScrapService
{
    private readonly IConfiguration _configuration = Configuration.Default.WithDefaultLoader();
    private const string ListAddress = "https://podzial.mech.pk.edu.pl/stacjonarne/kopia/2022-2023/zima/lista.html";

    public Task<IDocument> ScrapGroupsTable() => BrowsingContext.New(_configuration).OpenAsync(ListAddress);

    public Task<IDocument> ScrapSchedule(string group) => BrowsingContext.New(_configuration).OpenAsync($"https://podzial.mech.pk.edu.pl/stacjonarne/kopia/2022-2023/zima/plany/{group}");
}