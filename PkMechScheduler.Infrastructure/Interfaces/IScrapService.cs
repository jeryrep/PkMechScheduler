using AngleSharp.Dom;

namespace PkMechScheduler.Infrastructure.Interfaces;

public interface IScrapService
{
    public Task<IDocument> ScrapGroupsTeachersRoomsInfo();
    public Task<IDocument> ScrapSchedule(string linkEnd);
    public IEnumerable<IDocument> ScrapSchedules(IEnumerable<string> linkEnds);
}