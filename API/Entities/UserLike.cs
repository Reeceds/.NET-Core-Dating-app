namespace API.Entities;

public class UserLike
{
    public AppUser SourceUser { get; set; } // Curent user
    public int SourceUserId { get; set; }
    public AppUser TargetUser { get; set; } // Being liked by the source user
    public int TargetUserId { get; set; }
}
