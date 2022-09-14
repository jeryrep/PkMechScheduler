using AngleSharp.Dom;

namespace PkMechScheduler.Frontend.Interfaces;

public interface ISerializerService
{
    public Task AddGroupsToDb(IDocument document);
    public Task AddScheduleToDb(IDocument document);
}