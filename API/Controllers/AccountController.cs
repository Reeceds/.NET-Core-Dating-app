using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.Username.ToLower();

        var result = await _userManager.CreateAsync(user, registerDto.Password); // Save user into db

        if (!result.Succeeded) return BadRequest(result.Errors); // Checks if the user was successfully saved in the db

        var roleResult = await _userManager.AddToRoleAsync(user, "Member"); // Adds user to a role in db

        if(!roleResult.Succeeded) return BadRequest(result.Errors);
        
        return new UserDto // Return the new registered user
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")] // POST: api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users
            .Include(p => p.Photos) // Access related entity and include the photos of this user
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username); // Gets the user that matvhes the username

        if (user == null) return Unauthorized("Invalid unnsername");

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password); // Check if the password mathces

        if (!result) return Unauthorized("Invalid password"); // If password incorrect then return

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
