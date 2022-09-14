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

    public async Task<Dictionary<string, string>> GetGroups()
    {
        if (!_context.Groups.AnyAsync().Result)
            await _serializerService.AddGroupsToDb(_scrapService.ScrapGroupsTable().Result);
        return await _context.Groups.ToDictionaryAsync(x => x.Name, y => y.Link);
    }

    public async Task<List<BlockModel>> GetBlocks(string courseKey)
    {
        if (_context.Blocks.AnyAsync().Result && Preferences.Get("Course", string.Empty) == courseKey)
            return await _context.Blocks.ToListAsync();
        await ClearBlocksTable();
        var links = await _context.Groups.Where(x => x.Name.Contains(courseKey)).Select(x => x.Link).ToListAsync();
        foreach (var link in links)
            await _serializerService.AddScheduleToDb( _scrapService.ScrapSchedule(link).Result);


        return await _context.Blocks.ToListAsync();
    }

    private async Task ClearBlocksTable()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE From Blocks;");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='Blocks';");
        _context.ChangeTracker.Clear();
        await _context.SaveChangesAsync();
    }
}