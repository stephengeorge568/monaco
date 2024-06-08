using System.Globalization;
using server.models;

namespace Monaco.Services.Interfaces;

public interface IDocumentService
{
    int RevisionId { get; set; }

    string GetModel();

    Queue<Operation> ChangeQueue { get; set; }

    Dictionary<int, List<Operation>> GetHistory();
    Dictionary<int, List<Operation>> GetPreHistory();

    List<Operation> CommitChange(Operation op);

    void Reset();
}