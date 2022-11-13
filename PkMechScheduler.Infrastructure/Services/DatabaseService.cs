using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Enums;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Infrastructure.Interfaces;

namespace PkMechScheduler.Infrastructure.Services;

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
        var document = _scrapService.ScrapGroupsTeachersRoomsInfo().Result;
        var groups = _serializerService.GetGroupListFromDocument(document);
        await _context.Groups.AddRangeAsync(groups);
        var teachers = _serializerService.GetTeacherListFromDocument(document);
        await _context.Teachers.AddRangeAsync(teachers);
        var rooms = _serializerService.GetRoomListFromDocument(document);
        await _context.Rooms.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();
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

    public async Task<IEnumerable<StudentBlock>> GetBlocks(string courseKey, string preference, bool force = false)
    {
        if (await _context.StudentBlocks.AnyAsync() && preference == courseKey && !force)
            return await _context.StudentBlocks.ToListAsync();
        await ClearTable(nameof(_context.StudentBlocks));
        var links = await _context.Groups.Where(x => x.Name.Contains(courseKey)).Select(x => x.Link).ToListAsync();
        var list = _serializerService.ConvertDocumentsToBlockList(_scrapService.ScrapSchedules(links), Preference.Student);
        await _context.StudentBlocks.AddRangeAsync(list.OfType<StudentBlock>());
        await _context.SaveChangesAsync();
        return await _context.StudentBlocks.ToListAsync();
    }

    public async Task<IEnumerable<TeacherBlock>> GetTeacherBlocks(string teacher, string preference, bool force = false)
    {
        if (await _context.TeacherBlocks.AnyAsync() && preference == teacher && !force)
            return await _context.TeacherBlocks.ToListAsync();
        await ClearTable(nameof(_context.TeacherBlocks));
        var links = await _context.Teachers.Where(x => x.Name!.Contains(teacher)).Select(x => x.Link).ToListAsync();
        var list = _serializerService.ConvertDocumentsToBlockList(_scrapService.ScrapSchedules(links!), Preference.Teacher);
        await _context.TeacherBlocks.AddRangeAsync(list.OfType<TeacherBlock>());
        await _context.SaveChangesAsync();
        return await _context.TeacherBlocks.ToListAsync();
    }

    public async Task SaveTeacherBlocksToDb(string teacher)
    {
        await ClearTable(nameof(_context.TeacherBlocks));
        var links = await _context.Teachers.Where(x => x.Name!.Contains(teacher)).Select(x => x.Link).ToListAsync();
        var list = _serializerService.ConvertDocumentsToBlockList(_scrapService.ScrapSchedules(links!), Preference.Teacher);
        await _context.TeacherBlocks.AddRangeAsync(list.OfType<TeacherBlock>());
        await _context.SaveChangesAsync();
    }

    private async Task ClearTable(string table)
    {
        await _context.Database.ExecuteSqlRawAsync($"DELETE From {table};");
        await _context.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name='{table}';");
        _context.ChangeTracker.Clear();
        await _context.SaveChangesAsync();
    }
}