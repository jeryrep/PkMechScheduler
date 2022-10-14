using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Frontend.Interfaces;

public interface IDatabaseService
{
    public Task<Dictionary<string, string>> GetGroups(bool force = false);
    public Task<Dictionary<string, string>> GetTeachers(bool force = false);
    public Task<List<StudentBlock>> GetBlocks(string courseKey, bool force = false);
    public Task<List<TeacherBlock>> GetTeacherBlocks(string teacher, bool force = false);
    public Task SaveTeacherBlocksToDb(string teacher);
}