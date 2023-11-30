using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) // Built in method, must spell conrrect 'OnModelCreating'
    {
        base.OnModelCreating(builder);

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
    }
}
