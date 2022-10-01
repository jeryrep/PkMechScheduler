using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SchedulerContext _context;
    private readonly ISerializerService _serializerService;
    private readonly IScrapService _scrapService;
    public DatabaseService(SchedulerContext context, ISerializerService serializerService, IScrapService scrapService)
    {
        _context = context;
        _serializerService = serializerService;
        _scrapService = scrapService;
    }

    private async Task ScrapAgain()
    {
        await ClearTable(nameof(_context.Groups));
        await ClearTable(nameof(_context.Rooms));
        await ClearTable(nameof(_context.Teachers));
        await _serializerService.AddGroupsToDb(_scrapService.ScrapGroupsTable().Result);
    }

    public async Task<Dictionary<string, string>> GetGroups(bool force = false)
    {
        if (await _context.Groups.AnyAsync() && !force)
            return await _context.Groups.ToDictionaryAsync(x => x.Name, y => y.Link);
        await ScrapAgain();
        return await _context.Groups.ToDictionaryAsync(x => x.Name, y => y.Link);
    }

    public async Task<Dictionary<string, string>> GetTeachers(bool force = false)
    {
        if (await _context.Teachers.AnyAsync() && !force)
            return _context.Teachers.AsEnumerable().DistinctBy(x => x.Name).ToDictionary(x => x.Name, y => y.Link);
        await ScrapAgain();
        return _context.Teachers.AsEnumerable().DistinctBy(x => x.Name).ToDictionary(x => x.Name, y => y.Link);
    }

    public async Task<List<BlockModel>> GetBlocks(string courseKey, bool force = false)
    {
        if (_context.Blocks.AnyAsync().Result && Preferences.Get("Course", string.Empty) == courseKey && !force)
            return await _context.Blocks.ToListAsync();
        await ClearTable(nameof(_context.Blocks));
        var links = await _context.Groups.Where(x => x.Name.Contains(courseKey)).Select(x => x.Link).ToListAsync();
        foreach (var link in links)
            await _serializerService.AddScheduleToDb( _scrapService.ScrapSchedule(link).Result);
        return await _context.Blocks.ToListAsync();
    }

    private async Task ClearTable(string table)
    {
        await _context.Database.ExecuteSqlRawAsync($"DELETE From {table};");
        await _context.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name='{table}';");
        _context.ChangeTracker.Clear();
        await _context.SaveChangesAsync();
    }
}