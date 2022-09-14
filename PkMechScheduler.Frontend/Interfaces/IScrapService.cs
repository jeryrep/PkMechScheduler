using AngleSharp.Dom;

namespace PkMechScheduler.Frontend.Interfaces;

public interface IScrapService
{
    public Task<IDocument> ScrapGroupsTable();
    public Task<IDocument> ScrapSchedule(string group);
}