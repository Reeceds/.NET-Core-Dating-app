namespace API.Entities;

public class Connection
{
    public Connection() // Empty constructor for entity framework, when schema is created for db so that when it creates this new 'Connection' class, it won't try to pass the 'connectionId' as well
    {
        
    }
    public Connection(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }
    public string ConnectionId { get; set; }
    public string Username { get; set; }
}
