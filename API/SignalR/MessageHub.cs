using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> _presenceHub)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        this._presenceHub = _presenceHub;
    }

    public override async Task OnConnectedAsync() // Overrides the 'OnConnectedAsync' function in PresenceHub.cs
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"]; // query string for the users username
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName); // Adds group to SignalR group
        var group = await AddToGroup(groupName); // Adds group to the db

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages); // Users will receive messages from signalR istead of an API call
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup(); // Removes group from db
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception); // On disconnect, group is auto removed from SignalR group
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself."); // If the current user is trying to send message to themself. Throw exception as we don't have access to http responses in Hub
        
        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) throw new Exception("Not found user");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        var group = await _messageRepository.GetMessageGroup(groupName);

        // Checks if two users from a group are active on the messaging section and updates the timestamp of the messages to be the current time (functionality for messages being displayed as 'message read')
        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            // Access PresenceHub and send user a new message notification if are not currently conencted to the message hub
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }

        _messageRepository.AddMessage(message); // Add this new message to the message repo

        if (await _messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    private string GetGroupName(string caller, string other) // Sorts connected users into alpahabetical order
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName); // Gets the group
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername()); // Create a new connection

        if (group == null)
        // If there is no group, create a new one and add it to the message repository
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection); // Add the new connection to the existing or new group

        if (await _messageRepository.SaveAllAsync()) return group; // SaveAllAsync() return a bool which is what this AddToGroup() function returns

        throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        // Removes connection from db. Does not remove from messageGroup in SignalR
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }
}
