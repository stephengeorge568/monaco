namespace Monaco.Services;

using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Monaco.Hubs;
using Monaco.Services.Interfaces;
using Monaco.Utility;
using server.models;

/**
    In a production environment, many of the properties (Model, history, etc) of this service may be
    extracted out into other services/systems in order to allow scaling both in terms of # of documents
    able to be editted as well as in terms of application scalability. For the purposes of this Test
    implementation, I will only support one document model at a time - which is stored locally in memory
    as a property of this singleton.
*/
public class TestDocumentService : IDocumentService
{
    public int RevisionId { get; set; }

    public Queue<Operation> ChangeQueue { get; set; }

    public Dictionary<int, List<Operation>> History { get; set; }

    public Dictionary<int, List<Operation>> PreHistory { get; set; }

    public string Model { get; set; }

    private readonly IHubContext<OpHub> _hubContext;

    public TestDocumentService(IHubContext<OpHub> hubContext)
    {
        _hubContext = hubContext;
        RevisionId = 0;
        Model = "";
        History = new Dictionary<int, List<Operation>> {};
        PreHistory = new();
        ChangeQueue = new();
    }

    public async Task<int> CommitChange(Operation op, string documentId)
    {
        ChangeQueue.Enqueue(op);
        WaitForTurn(op.Id);

        var transformedOps = Transformer.Transform(op, History);

        // Commit change to model.
        // Any revision to model increments document revision Id, and the Operation responsible
        // for a given revision will also share that new revisionId.
        RevisionId++;
        foreach (var newOp in transformedOps)
        {
            Model = ModelUtility.UpdateModel(Model, newOp);
            newOp.RevisionId = RevisionId;
        }

        CommitChangesToHistory(History, transformedOps);

        await Task.WhenAll(transformedOps.Select(o => PropogateOperationToGroup(o, documentId)));
        
        ChangeQueue.Dequeue();
        return RevisionId;
    }

    public void Reset()
    {
        History = new Dictionary<int, List<Operation>>();
        PreHistory = new Dictionary<int, List<Operation>>();
        RevisionId = 0;
        Model = "";
        ChangeQueue = new();
    }

    private void CommitChangesToHistory(Dictionary<int, List<Operation>> history, List<Operation> operations)
    {
        if (!history.ContainsKey(operations.First().RevisionId))
        {
            history[operations.First().RevisionId] = [];
        }
        history[operations.First().RevisionId].AddRange(operations);
    }

    private void WaitForTurn(string id)
    {
        while (id != ChangeQueue.Peek().Id)
        {
            Thread.Sleep(100);
        }
    }

    private async Task PropogateOperationToGroup(Operation operation, string group){
        await _hubContext.Clients.Group(group).SendAsync("operationRecieved", operation);
    }
}