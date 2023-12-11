using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        // Gets a list of user and their roles
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")] // Technically should be PUT method as this updates data in the db. Uased a POST instead as we want to return an updated list of roles
    public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role.");

        var selectedRoles = roles.Split(",").ToArray(); // Put the roles from the query into an array

        var user = await _userManager.FindByNameAsync(username); // Get the user we are trying to modify

        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user); // Get the existing user roles that the user is inside of

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles)); // Gets the user roles from the query that the user does not have and adds them to those roles

        if(!result.Succeeded) return BadRequest("Failed to add to roles.");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles)); // Removes the user from any roles they have but are not included of the 'selectedRoles' list from the query

        if(!result.Succeeded) return BadRequest("Failed to remove from roles.");

        return Ok(await _userManager.GetRolesAsync(user));
    }
    
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Admins or moderators can see this");
    }
}
