using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Frontend.Interfaces;

public interface IDatabaseService
{
    public Task<Dictionary<string, string>> GetGroups(bool force = false);
    public Task<Dictionary<string, string>> GetTeachers(bool force = false);
    public Task<List<BlockModel>> GetBlocks(string courseKey, bool force = false);

}