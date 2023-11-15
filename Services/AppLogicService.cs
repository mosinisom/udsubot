public class AppLogicService
{
    DbService _db;
    ILogger<AppLogicService> _log;

    public AppLogicService(DbService db, ILogger<AppLogicService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task BlockUser(string telegramlink)
    {
        Users user = await _db.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;
        user.HasAccess = false;
        await _db.UpdateUser(user);
    }
    public async Task UnblockUser(string telegramlink)
    {
        Users user = await _db.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;
        user.HasAccess = true;
        await _db.UpdateUser(user);
    }

    public async Task BlockUser(int chat_id)
    {
        Users? user = await _db.GetUserByChatId(chat_id);
        if (user == null)
            return;
        user.HasAccess = false;
        await _db.UpdateUser(user);
    }

    public async Task UnblockUser(int chat_id)
    {
        Users? user = await _db.GetUserByChatId(chat_id);
        if (user == null)
            return;
        user.HasAccess = true;
        await _db.UpdateUser(user);
    }
}