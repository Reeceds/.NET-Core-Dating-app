using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> // These are specified beacsue of the Id's for 'AppUser & AppRole' being set to int and also because of the join table <AppUserRole>. Make sure these are are in this order.
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) // Built in method, must spell conrrect 'OnModelCreating'
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles) // AppUser has many UserRoles
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired(); // ForeignKey can't be null
        
        builder.Entity<AppRole>() // Flipside of the above relationship
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetUserId }); // Represents primary key used inside the Likes table

        builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser) // A single user e.g. Lisa
            .WithMany(l => l.LikedUsers) // SourceUser 'Lisa' can like many other users e.g. Tom, Ben, Jack
            .HasForeignKey(s => s.SourceUserId) // Specify a foreignKey
            .OnDelete(DeleteBehavior.Cascade); // If user deletes their profile, this will delete their likes as well
        
        builder.Entity<UserLike>()
            .HasOne(s => s.TargetUser) // User that has been liked
            .WithMany(l => l.LikedByUsers) // Users that have liked this target user
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Message>()
            .HasOne(u => u.Recipient) // A recipient
            .WithMany(m => m.MessagesReceived) // Can receive many messages
            .OnDelete(DeleteBehavior.Restrict); // Still want the recipient of a message to be able to view a message even if the sender has deleted their profile
        
        builder.Entity<Message>()
            .HasOne(u => u.Sender) // A sender
            .WithMany(m => m.MessagesSent) // Can send many messages
            .OnDelete(DeleteBehavior.Restrict);
    }
}
