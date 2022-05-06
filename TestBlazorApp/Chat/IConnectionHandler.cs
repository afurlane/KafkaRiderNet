namespace TestBlazorApp.Chat
{
    public interface IConnectionHandler
    {
        void AddClient(string connectionId);
        void RemoveClient(string connectionId);
        int GetConnectedClients();
    }
}