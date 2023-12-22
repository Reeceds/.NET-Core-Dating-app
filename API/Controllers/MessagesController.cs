using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public MessagesController(IMapper mapper, IUnitOfWork uow)
    {
        _mapper = mapper;
        _uow = uow;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself."); // If the current user is trying to send message to themself
        
        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) return NotFound();

        var message = new Message{
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _uow.MessageRepository.AddMessage(message); // Add this new message to the message repo

        if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams) // Returns a 'PagedList' of 'MessageDtos'
    {
        messageParams.Username = User.GetUsername();

        var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

        return messages;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await _uow.MessageRepository.GetMessage(id); // Gets the selected message

        if (message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized(); // Checks if the user thats attempting to delete is actually associated with the message, returns if not

        if (message.SenderUsername == username) message.SenderDeleted = true; // Checks if the message sender has deleted a message, if so then 'SenderDeleted' is set to true
        if (message.RecipientUsername == username) message.RecipientDeleted = true; // Checks if the message receiver has deleted a message, if so then 'RecipientDeleted' is set to true

        if (message.SenderDeleted && message.RecipientDeleted) 
        {
            _uow.MessageRepository.deleteMessage(message); // If both sender/receiver has deleted the same message, then delete it
        }

        if (await _uow.Complete()) return Ok(); // Update the db, return 'Ok()' if update was successful

        return BadRequest("Problem deleting the message"); // Return 'BadRequest' if the update attempt failed
    }
}
