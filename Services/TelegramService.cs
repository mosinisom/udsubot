using Telegram.Bot;

class TelegramService
{
    DbService _db;
    IConfiguration _cfg;
    ILogger<TelegramService> _log;
    TelegramBotClient _bot;
    CancellationTokenSource _cts;

    public TelegramService(DbService db, IConfiguration cfg, ILogger<TelegramService> log)
    {
        _db = db;
        _cfg = cfg;
        _log = log;
        _bot = new (_cfg.GetValue<string>("BotSecret"));
        _cts = new ();
    }

    // ...

}