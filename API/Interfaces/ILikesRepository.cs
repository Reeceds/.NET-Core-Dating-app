using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId); // returns a userLike. SourceUserId & targetUserId make up the primary key of the entity inside the 'Likes' table
    Task<AppUser> GetUserWithLikes(int userId); // Return AppUser entity with additional included entities
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams); // Returns a list of LikeDto's. 'predicate' = the users they have liked or the users they are liked by - to be determined via logic on the function. 'userId' could be either sourceUserId or targetUserId - to be determined via logic on the function.
}
