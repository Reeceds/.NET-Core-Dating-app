using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = _context.Users.AsQueryable();

        query = query.Where(u => u.UserName != userParams.CurrentUsername); // Removes logged in user from matches results
        query = query.Where(u => u.Gender == userParams.Gender); 

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1)); // minDob refers to oldest dob
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge)); // maxDob referes to youngest dob

        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob); // Query for results absed on age

        query = userParams.OrderBy switch // Query for ordering results (this is the syntax for a c# switch statement: https://stackoverflow.com/questions/8155772/setting-a-variable-to-a-switchs-result)
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive) // '_' represents the default case 
        };

        return await PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider), 
            userParams.PageNumber, 
            userParams.PageSize); 
            // Able to use static CreateAsync() method from PagedList class, this returns all items and their properties
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos) // Gets the associated photo data as well as the user
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(p => p.Photos) // Gets the associated photo data as well as the users
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }
}
