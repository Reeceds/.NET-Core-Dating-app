using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers (UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return; // If there are users in the db then do not sync seed data

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        var roles = new List<AppRole> // Create new roles
        {
            new AppRole{Name = "Member"},
            new AppRole{Name = "Admin"},
            new AppRole{Name = "Moderator"},
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role); // Add the roles to the role manager
        }

        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member"); // Add users to the role
        }

        var admin = new AppUser
        {
            UserName = "admin"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd"); // Create admin user
        await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"}); // Give admin user multiple roles (Admin & Moderator)
    }
}
