using Microsoft.AspNetCore.SignalR;
using server.models;

namespace SignalRChat.Hubs
{
    public class OpHub : Hub
    {
        private Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        
        public async Task NewOperation(Operation operation, string documentId)
        {
            // Transform incoming operation ...
            Console.WriteLine(Context.ConnectionId);
            var newRevisionId = 2;
            await PropogateOperationToGroup(operation, documentId);
            await Clients.Caller.SendAsync("operationTransformedAck", newRevisionId);

        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
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