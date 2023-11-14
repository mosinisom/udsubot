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

    public async Task<Users?> GetUserByChatId(long chatId)
    {
        return await _context.
            Users.FirstOrDefaultAsync(u => u.Chat_ID == chatId);
    }

    public async Task AddUser(long Chat_ID)
    {
        if (await GetUserByChatId(Chat_ID) != null)
            return;

        Users user = new()
        {
            Chat_ID = Chat_ID,
            HasAccess = false,
            RegistrationDate = DateTime.Now,
            StudentCardNumber = 0
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUser(Users user)
    {
        if(user == null)
            return;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    // под вопросом эти две функции -----------------------------
    public async Task BlockUser(Users user)
    {
        user.HasAccess = false;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task UnblockUser(Users user)
    {
        user.HasAccess = true;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    // ----------------------------------------------------------

    public async Task AddStudent(Students student)
    {
        if (await GetStudentByUserId(student.User_ID) != null)
            return;
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task<Students?> GetStudentByUserId(long User_ID)
    {
        return await _context.
            Students.FirstOrDefaultAsync(s => s.User_ID == User_ID);
    }

    public async Task UpdateStudent(Students student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task<Students?> GetStudentByTelegramLink(string TelegramLink)
    {
        return await _context.
            Students.FirstOrDefaultAsync(s => s.TelegramLink == TelegramLink);
    }

    public async Task<List<Students>> GetRandomStudents()
    {
        return await _context.Students
            .FromSqlRaw("SELECT * FROM students ORDER BY RANDOM() LIMIT 5")
            .ToListAsync();
    }

    public async Task<List<Students>> GetAllStudents()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task SendMessage(Messages message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<Messages?> ReceiveMessage(int ToStudent_ID)
    {
        return await _context.
            Messages.Where(m => m.ToStudent_ID == ToStudent_ID)
            .OrderByDescending(m => m.Message_ID)
            .FirstOrDefaultAsync();
    }

    public async Task LikeStudent(Likes like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Likes>> GetAllMyLikes(int ToStudent_ID)
    {
        return await _context.Likes
            .Where(l => l.ToStudent_ID == ToStudent_ID)
            .ToListAsync();
    }

    public async Task<int> GetLikesCount(int ToStudent_ID)
    {
        return await _context.Likes
            .Where(l => l.ToStudent_ID == ToStudent_ID)
            .CountAsync();
    }

    public async Task<List<Students>> GetStudentsByInstitute(int Institute_ID)
    {
        return await _context.Students
            .Where(s => s.Institute_ID == Institute_ID)
            .ToListAsync();
    }

    public async Task<int> GetCountOfStudentsByInstitute(int Institute_ID)
    {
        return await _context.Students
            .Where(s => s.Institute_ID == Institute_ID)
            .CountAsync();
    }

    public async Task AddPhoto(Photos photo)
    {
        await _context.Photos.AddAsync(photo);
        await _context.SaveChangesAsync();
    }

    public async Task<Photos?> GetPhoto(int Student_ID)
    {
        return await _context.Photos
            .Where(p => p.Student_ID == Student_ID)
            .OrderByDescending(p => p.Photo_ID)
            .FirstOrDefaultAsync();
    }

    public async Task SendMessageToAllStudents(string message)
    {
        List<Students> students = await GetAllStudents();
        foreach (var student in students)
        {
            Messages msg = new()
            {
                FromStudent_ID = 0,
                ToStudent_ID = student.Student_ID,
                Text = message,
                Date = DateTime.Now
            };
            await SendMessage(msg);
        }
    }







}