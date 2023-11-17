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

    public async Task AddUser(long Chat_ID, string username)
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

        Students student = new()
        {
            Name = username,
            TelegramLink = username,
            User_ID = user.User_ID,
            Institute_ID = 0,
            Year = 0,
            Description = ""
        };

        await _context.Users.AddAsync(user);
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUser(Users user)
    {
        if(user == null)
            return;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<Users> GetUserByTelegramLink (string TelegramLink)
    {
        Students? student = await GetStudentByTelegramLink(TelegramLink);

    // придумать что-то с этим
        if (student == null)
            return null;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.User_ID == student.User_ID);
        
        // придумать что-то с этим
        if (user == null)
            return null; 

        return user;


    }

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

    public async Task LikeStudent(int from_id, int to_id)
    {
        Likes like = new()
        {
            FromStudent_ID = from_id,
            ToStudent_ID = to_id,
            Date = DateTime.Now
        };
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Likes>> GetAllMyLikes(long chatId)
    {
        Users? user = await GetUserByChatId(chatId);
        if (user == null)
            return null;

        Students? student = await GetStudentByUserId(user.User_ID);
        if (student == null)
            return null;

        return await _context.Likes
            .Where(l => l.FromStudent_ID == student.Student_ID)
            .ToListAsync();
    }

    public async Task<int> GetLikesCount(long chatId)
    {
        Users? user = await GetUserByChatId(chatId);
        if (user == null)
            return 0;

        Students? student = await GetStudentByUserId(user.User_ID);
        if (student == null)
            return 0;
        
        return await _context.Likes
            .Where(l => l.ToStudent_ID == student.Student_ID)
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

    public async Task<string?> GetPhotoPath(int User_ID)
    {
        return await _context.Photos
            .Where(p => p.User_ID == User_ID)
            .OrderByDescending(p => p.Photo_ID)
            .Select(p => p.Path)
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