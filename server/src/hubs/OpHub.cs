using Microsoft.AspNetCore.SignalR;
using Monaco.Services.Interfaces;
using server.models;

namespace Monaco.Hubs
{
    public class OpHub : Hub
    {
        private Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        private IDocumentService _documentService;
        public OpHub(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        
        public async Task NewOperation(Operation operation, string documentId)
        {
            Console.WriteLine(operation.ToString);
            if (!_documentService.GetPreHistory().ContainsKey(operation.RevisionId))
            {
                _documentService.GetPreHistory().Add(operation.RevisionId, new List<Operation>());
            }
            _documentService.GetPreHistory()[operation.RevisionId].Add(operation.DeepCopy());
            var transformedOps = _documentService.CommitChange(operation);
            Console.WriteLine(transformedOps[0].ToString);
            Console.WriteLine('\n');
            await Task.WhenAll(transformedOps.Select(o => PropogateOperationToGroup(o, documentId)));
            await Clients.Caller.SendAsync("operationTransformedAck", _documentService.RevisionId);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("Client disconnected");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "BigTimeIdOhYeah");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (groups.ContainsKey(groupName)) {
                groups.Add(groupName, new HashSet<string>{Context.ConnectionId});
            } else {
                HashSet<string>? members;
                groups.TryGetValue(groupName, out members);
                members?.Add(Context.ConnectionId);
                groups.Add(groupName, members);
            }
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (groups.ContainsKey(groupName)) {
                HashSet<string>? members;
                groups.TryGetValue(groupName, out members);
                members?.Remove(Context.ConnectionId);
            }
        }

        public async Task PropogateOperationToGroup(Operation operation, string group){
            Console.WriteLine($"Propogating to {group}");
            await Clients.Group(group).SendAsync("operationRecieved", operation);
        }
    }
}