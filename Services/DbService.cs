using Microsoft.EntityFrameworkCore;

public class DbService
{
    private readonly DataContext _context;

    public DbService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Users>> GetUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<Users?> GetUserByChatIdAsync(long chatId)
    {
        return await _context.
            Users.FirstOrDefaultAsync(u => u.Chat_ID == chatId);
    }



    
}