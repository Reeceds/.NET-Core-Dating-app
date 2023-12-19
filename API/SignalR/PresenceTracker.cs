namespace API.SignalR;

public class PresenceTracker
{
    // This class usese a dictionary to keep track of the users connected to the presenceHub
    private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>(); // Dictionary<string (key), List<string> (value)>

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;
        lock(OnlineUsers) // locks the Dictionary while this functionality is being handled to stop multiple users connected at the same time (dictionary is not threadsafe)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId); // If uer already has a key inside dictionary, add them
            }
            else
            {
                OnlineUsers.Add(username, new List<string>{connectionId}); // If user does not already have a key inside dictionary, create new key and add them
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        bool isOffline = false;
        lock(OnlineUsers) // locks the Dictionary while this functionality is being handled to stop multiple users connected at the same time (dictionaries are not threadsafe)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline); // If user is not in dictioanry, return

            OnlineUsers[username].Remove(connectionId); // Removes connectionId from the key

            if (OnlineUsers[username].Count == 0) // If user is offline
            {
                OnlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock(OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray(); // Gets alphabetical list of users by their usernames (key)

            return Task.FromResult(onlineUsers);
        }
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;

        lock (OnlineUsers)
        {
            connectionIds = OnlineUsers.GetValueOrDefault(username);// Gets a list of the connections for this user
        }

        return Task.FromResult(connectionIds);
    }
}
