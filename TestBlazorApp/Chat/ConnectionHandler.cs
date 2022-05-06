namespace TestBlazorApp.Chat
{
    public class ConnectionHandler : IConnectionHandler
    {
        private static List<string> clients = new List<string>();
        public void AddClient(string connectionId)
        {
            lock (clients)
                clients.Add(connectionId);
            
        }

        public int GetConnectedClients()
        {
            // Lock should not be needed...
            return clients.Count;
        }

        public void RemoveClient(string connectionId)
        {
            lock(clients)
                clients.Remove(connectionId);
        }
    }
}
