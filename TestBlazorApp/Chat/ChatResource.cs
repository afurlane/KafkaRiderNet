using Microsoft.AspNetCore.SignalR;

namespace TestBlazorApp.Chat
{
    public class ChatResource : Hub
    {
        public const string ChatURL = "/chat";
        private readonly ILogger<ChatResource> logger;
        private readonly IConnectionHandler connectionHandler;

        public ChatResource(ILogger<ChatResource> logger, IConnectionHandler connectionHandler)
        {
            this.logger = logger;
            this.connectionHandler = connectionHandler;
        }

        public void Send(string userName, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.SendAsync("ChatMessage", userName, message);
        }
        public override Task OnConnectedAsync()
        {

            String connId = Context.ConnectionId;
            connectionHandler.AddClient(connId);
            logger.LogInformation($"{connId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            string connId = Context.ConnectionId;
            connectionHandler.RemoveClient(connId);
            logger.LogInformation($"Disconnected {e?.Message} {connId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
