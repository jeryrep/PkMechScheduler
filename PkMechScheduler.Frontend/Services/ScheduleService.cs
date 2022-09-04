using AngleSharp;
using PkMechScheduler.Frontend.Models;

namespace PkMechScheduler.Frontend.Services;

public class ScheduleService
{
    private readonly SerializerService _serializerService;
    private readonly IConfiguration _configuration = Configuration.Default.WithDefaultLoader();

    public ScheduleService(SerializerService serializerService)
    {
        _serializerService = serializerService;
    }

    public Dictionary<string, string> GetGroups()
    {
        const string address = "https://podzial.mech.pk.edu.pl/stacjonarne/kopia/2022-2023/zima/lista.html";
        var document = BrowsingContext.New(_configuration).OpenAsync(address).Result;
        var table = document.QuerySelector("div#oddzialy");
        return _serializerService.SerializeGroups(table!);
    }
    public Dictionary<Day, List<BlockModel>> GetRawSchedule(string group)
    {
        var address = $"https://podzial.mech.pk.edu.pl/stacjonarne/kopia/2022-2023/zima/plany/{group}";
        var document = BrowsingContext.New(_configuration).OpenAsync(address).Result;
        var table = document.QuerySelectorAll("table").FirstOrDefault(x => x.ClassList.Contains("tabela"));
        return _serializerService.SerializeScheduleToJson(table!);
    }
}