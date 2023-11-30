using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")] // Username of the user they are about to like
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId(); // Get Id of the current active user
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        if (likedUser == null) return NotFound();

        if (sourceUser.UserName == username) return BadRequest("You cannot like yourself.");

        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

        if (userLike != null) return BadRequest("You already like this user");

        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams) // When sending a query string as params, you msut specify [FromQuery] as it sends an object instead of a string
    {
        likesParams.UserId = User.GetUserId();

        var users = await _likesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages)); // Gets a pagination header

        return Ok(users);
    }
    
}
