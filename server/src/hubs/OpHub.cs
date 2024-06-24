using Microsoft.AspNetCore.SignalR;
using Monaco.Services.Interfaces;
using server.models;

namespace Monaco.Hubs
{
    public class OpHub : Hub
    {
        private IDocumentService _documentService;
        public OpHub(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public async Task NewOperation(Operation operation, string documentId)
        {        
            _documentService.ChangeQueue.Enqueue(operation);
            var newRevisionId = await _documentService.CommitChange(operation, documentId);
            await Clients.Caller.SendAsync("operationTransformedAck", newRevisionId);
            _documentService.ChangeQueue.Dequeue();
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
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task PropogateOperationToGroup(Operation operation, string group){
            await Clients.Group(group).SendAsync("operationRecieved", operation);
        }
    }
}