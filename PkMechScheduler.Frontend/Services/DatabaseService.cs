using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
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

    public async Task<List<StudentBlock>> GetBlocks(string courseKey, bool force = false)
    {
        if (await _context.StudentBlocks.AnyAsync() && Preferences.Get(nameof(Preference.Course), string.Empty) == courseKey && !force)
            return await _context.StudentBlocks.ToListAsync();
        await ClearTable(nameof(_context.StudentBlocks));
        var links = await _context.Groups.Where(x => x.Name.Contains(courseKey)).Select(x => x.Link).ToListAsync();
        foreach (var link in links)
            await _serializerService.AddScheduleToDb(_scrapService.ScrapSchedule(link).Result, Preference.Student);
        return await _context.StudentBlocks.ToListAsync();
    }

    public async Task<List<TeacherBlock>> GetTeacherBlocks(string teacher, bool force = false)
    {
        if (await _context.TeacherBlocks.AnyAsync() && Preferences.Get(nameof(Preference.Teacher), string.Empty) == teacher && !force)
            return await _context.TeacherBlocks.ToListAsync();
        await ClearTable(nameof(_context.TeacherBlocks));
        var links = await _context.Teachers.Where(x => x.Name.Contains(teacher)).Select(x => x.Link).ToListAsync();
        foreach (var link in links)
            await _serializerService.AddScheduleToDb(_scrapService.ScrapSchedule(link).Result, Preference.Teacher);
        return await _context.TeacherBlocks.ToListAsync();
    }

    private async Task ClearTable(string table)
    {
        await _context.Database.ExecuteSqlRawAsync($"DELETE From {table};");
        await _context.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name='{table}';");
        _context.ChangeTracker.Clear();
        await _context.SaveChangesAsync();
    }
}