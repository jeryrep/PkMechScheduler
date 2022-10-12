using AngleSharp.Dom;
using PkMechScheduler.Frontend.Enums;

namespace PkMechScheduler.Frontend.Interfaces;

public interface ISerializerService
{
    public Task AddGroupsToDb(IDocument document);
    public Task AddScheduleToDb(IDocument document, Preference mode);
}