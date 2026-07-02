using Microsoft.EntityFrameworkCore;
using TourPlanner.Entities;

namespace TourPlanner.Repositories;

public class UserRepository : BaseRepository
{
    public UserRepository(AppDBC dbc) : base(dbc)
    {
    }

    public virtual async Task<User?> GetUserByUsername(string username)
    {
        return await dbc.Users.SingleOrDefaultAsync(u => u.Username == username);
    }

    public virtual async Task AddUser(User user)
    {
        try
        {
            await dbc.Users.AddAsync(user);
            await dbc.SaveChangesAsync();
        }
        catch (Exception e) when (e is DbUpdateException || e is ArgumentException)
        {
            //TODO exchange with custom exception 
            throw new Exception($"User with username {user.Username} already exists", e);
        }
    }
}