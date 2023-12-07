using API.Helpers;

namespace API.Helpers;

public class MessageParams : PaginationParams
{
    public string Username { get; set; } // Current users username
    public string Container { get; set; } = "Unread"; // Return unred messages by default
}
