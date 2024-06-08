namespace Monaco.Services;

using System.Collections.Generic;
using Monaco.Services.Interfaces;
using Monaco.Utility;
using server.models;

public class TestDocumentService : IDocumentService
{
    public int RevisionId { get; set; }

    public Queue<Operation> ChangeQueue { get; set; }

    // Look into conccurrent dictionary
    private Dictionary<int, List<Operation>> history;

    private Dictionary<int, List<Operation>> historyPre;

    private string Model;

    private ITransformService _transformService;

    public TestDocumentService(ITransformService transformService)
    {
        RevisionId = 0;
        Model = "";
        history = new Dictionary<int, List<Operation>> { {0, [  ]}, };
        historyPre = new();
        ChangeQueue = new();
        _transformService = transformService;
    }

    public Dictionary<int, List<Operation>> GetHistory()
    {
        return history;
    }

    public Dictionary<int, List<Operation>> GetPreHistory()
    {
        return historyPre;
    }

    public string GetModel()
    {
        return Model;
    }

    // This shit needs to be rewritten.
    /*
        Transform incoming operations against the server's history.
        Apply the transformed operation to the document.
        Increment the server's revisionId.
        Assign the new revisionId to the operation.
        Add the operation to the server's history.
        Propagate the operation and new revisionId to all clients.
    */
    public List<Operation> CommitChange(Operation op)
    {
        ChangeQueue.Enqueue(op);
        var nextUpInQueue = ChangeQueue.Peek();
        while (op.Id != nextUpInQueue.Id)
        {
            Thread.Sleep(100);
        }

        var transformedOps = _transformService.Transform(op, history);

        RevisionId++;

        // This violates Microsoft's intent for Collection.All()
        // This sets all the RevIds in transformedOps to RevisionId
        // rewrite
        transformedOps.All(o => { o.RevisionId = RevisionId; return true; });

        if (!history.ContainsKey(transformedOps.First().RevisionId))
        {
            history[transformedOps.First().RevisionId] = [];
        }
        history[transformedOps.First().RevisionId].AddRange(transformedOps);
        foreach (var newOps in transformedOps)
        {
            Model = ModelUtility.UpdateModel(Model, newOps);
        }
        ChangeQueue.Dequeue();
        return transformedOps;
    }

    public void Reset()
    {
        history = new Dictionary<int, List<Operation>> { {0, [  ]}, };
        historyPre = new Dictionary<int, List<Operation>> { {0, [  ]}, };
        RevisionId = 0;
        Model = "";
        ChangeQueue = new();
    }
}