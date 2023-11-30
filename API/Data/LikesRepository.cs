using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;
    public LikesRepository(DataContext context)
    {
        _context = context;
    }

    public DataContext Context { get; }

    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        //! AsQueryable() means it is just a query and is not executed on db yet yet.
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable(); // Get a list of users from the db in order of username
        var likes = _context.Likes.AsQueryable(); // Get a list of the likes from the db as a query

        if (likesParams.Predicate == "liked") // Get the users that the source user has liked
        {
            likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
            users = likes.Select(like => like.TargetUser); // Filters users based on the returned likes list
        }

        if (likesParams.Predicate == "likedBy") // Get the users that have liked the source user
        {
            likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
            users = likes.Select(like => like.SourceUser);
        }

        var likedUsers = users.Select(user => new LikeDto // Gives a list of users based on the predicate
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
            City = user.City,
            Id = user.Id
        });

        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId) // Checks if a user has already been liked by another user
    {
        return await _context.Users
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
