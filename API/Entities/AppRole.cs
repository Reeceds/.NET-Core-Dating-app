using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppRole : IdentityRole<int> // Sets hidden Id property of the AppRoles to an int
{
    public ICollection<AppUserRole> UserRoles { get; set; } // Navigates to the join table which is the 'AppUserRole' collection. Can use 'ICollection' or 'List', both work in similar ways
}
