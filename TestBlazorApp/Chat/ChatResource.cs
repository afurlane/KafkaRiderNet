using Microsoft.AspNetCore.SignalR;

namespace TestBlazorApp.Chat
{
    public class ChatResource : Hub
    {
        public const string ChatURL = "/chat";
        public void Send(string userName, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.SendAsync("ChatMessage", userName, message);
        }
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
