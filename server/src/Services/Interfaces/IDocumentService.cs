using server.models;

namespace Monaco.Services.Interfaces;

public interface IDocumentService
{
    int RevisionId { get; }
    string Model { get; }
    Dictionary<int, List<Operation>> History { get; }
    Dictionary<int, List<Operation>> PreHistory { get; }
    Queue<Operation> ChangeQueue { get; set; }
    Task<int> CommitChange(Operation op, string documentId);
    void Reset();
}