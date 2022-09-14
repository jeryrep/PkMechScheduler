using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Frontend.Interfaces;

public interface IDatabaseService
{
    public Task<Dictionary<string, string>> GetGroups();
    public Task<List<BlockModel>> GetBlocks(string courseKey);
}