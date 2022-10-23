using AngleSharp.Dom;
using PkMechScheduler.Database.Enums;

namespace PkMechScheduler.Infrastructure.Interfaces;

public interface ISerializerService
{
    public Task AddGroupsToDb(IDocument document);
    public Task ConvertDocumentToBlockList(IDocument document, Preference mode);
    public Task ConvertDocumentsToBlockList(IEnumerable<IDocument> documents, Preference mode);
}