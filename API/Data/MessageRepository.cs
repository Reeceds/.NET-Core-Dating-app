using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void deleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = _context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false), // Show received messages, appart from ones the user has flagged for deletion
            "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false), // Show sent messages, appart from ones the user has flagged for deletion
            _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null && u.RecipientDeleted == false) // Show unread messages, appart from ones the user has flagged for deletion
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider); // Project query to a 'MessageDto'

        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize); // return 'MessageDto' as a pagedList
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var messages = await _context.Messages
            .Include(u => u.Sender).ThenInclude(p => p.Photos) // Gets photos. Include the 'Sender' as 'Photos' are a related entity for the 'AppUser' ('Sender' is a property of 'Message' entity & is of type 'AppUser')
            .Include(u => u.Recipient).ThenInclude(p => p.Photos) // Gets photos. Include the 'Recipient' as 'Photos' are a related entity for the 'AppUser' ('Recipient' is a property of 'Message' entity & is of type 'AppUser')
            .Where(
                m => m.RecipientUsername == currentUserName &&
                m.RecipientDeleted == false &&
                m.SenderUsername == recipientUserName ||
                m.RecipientUsername == recipientUserName &&
                m.SenderDeleted == false &&
                m.SenderUsername == currentUserName
            )
            .OrderBy(m => m.MessageSent) // Orders by latest messages at the bottom
            .ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList(); // Get messages that are unread by the current user. use 'ToList()' as there no need to access database again when 'ToListAsync()' was called above and is stored in memory

        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages) // When message thread is viewed (Message tab is clicked), any unread messages will be displayed to the user therefore updating the 'DateRead' field
            {
                message.DateRead = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(); // Save changed to db if any unread messages have now been updated and read
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
