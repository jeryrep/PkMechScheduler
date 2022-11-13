using AngleSharp.Dom;
using PkMechScheduler.Database.Enums;
using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Infrastructure.Interfaces;

public interface ISerializerService
{
    public IEnumerable<Group> GetGroupListFromDocument(IDocument document);
    public IEnumerable<Teacher> GetTeacherListFromDocument(IDocument document);
    public IEnumerable<Room> GetRoomListFromDocument(IDocument document);
    public List<BaseBlock> ConvertDocumentToBlockList(IDocument document, Preference mode);
    public List<BaseBlock> ConvertDocumentsToBlockList(IEnumerable<IDocument> documents, Preference mode);
}