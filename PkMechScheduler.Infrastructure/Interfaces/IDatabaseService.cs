using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Infrastructure.Interfaces;

public interface IDatabaseService
{
    public Task<Dictionary<string, string>> GetGroups(bool force = false);
    public Task<Dictionary<string, string>> GetTeachers(bool force = false);
    public Task<IEnumerable<StudentBlock>> GetBlocks(string courseKey, string preference, bool force = false);
    public Task<IEnumerable<TeacherBlock>> GetTeacherBlocks(string teacher, string preference, bool force = false);
    public Task SaveTeacherBlocksToDb(string teacher);
}