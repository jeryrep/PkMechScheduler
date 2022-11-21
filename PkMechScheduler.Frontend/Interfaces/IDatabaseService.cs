using MechScraper.Models;

namespace PkMechScheduler.Frontend.Interfaces;

public interface IDatabaseService
{
    public Task<Dictionary<string, string>> GetGroups(bool force = false);
    public Task<Dictionary<string, string>> GetTeachers(bool force = false);
    public Task<IEnumerable<StudentBlock>> GetBlocks(string courseKey, string preference, bool force = false);
    public Task<IEnumerable<TeacherBlock>> GetTeacherBlocks();
    public Task SaveTeacherBlocksToDb(string teacher);
}