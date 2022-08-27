using AngleSharp;
using PkMechScheduler.Api.Models;

namespace PkMechScheduler.Api.Services;

public class ScheduleService
{
    private readonly SerializerService _serializerService;

    public ScheduleService(SerializerService serializerService)
    {
        _serializerService = serializerService;
    }
    public async Task<Dictionary<Day, List<BlockModel>>> GetRawSchedule(string group)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var address = $"https://podzial.mech.pk.edu.pl/stacjonarne/html/plany/{group}.html";
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(address);
        var table = document.QuerySelectorAll("table").FirstOrDefault(x => x.ClassList.Contains("tabela"));
        return _serializerService.SerializeScheduleToJson(table!);
    }
}