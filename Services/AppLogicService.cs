using System.Reflection.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;

public class AppLogicService
{
    DbService _db;
    ILogger<AppLogicService> _log;

    public AppLogicService(DbService db, ILogger<AppLogicService> log)
    {
        _db = db;
        _log = log;
    }

    // public async Task AddUser(long chatId, DbService dbService)
    // {
    //     await dbService.AddUser(chatId);
    // }

    public async Task BlockUser(string telegramlink, DbService dbService)
    {
        Users user = await dbService.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;
        user.HasAccess = false;
        await dbService.UpdateUser(user);
    }
    public async Task UnblockUser(string telegramlink, DbService dbService)
    {
        Users user = await dbService.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;
        user.HasAccess = true;
        await dbService.UpdateUser(user);
    }

    public async Task BlockUser(long chat_id, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chat_id);
        if (user == null)
            return;
        user.HasAccess = false;
        await dbService.UpdateUser(user);
    }

    public async Task UnblockUser(long chat_id, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chat_id);
        if (user == null)
            return;
        user.HasAccess = true;
        await dbService.UpdateUser(user);
    }

    public async Task SendMessage(int from_id, int to_id, string text)
    {
        Messages message = new()
        {
            FromStudent_ID = from_id,
            ToStudent_ID = to_id,
            Text = text,
            Date = DateTime.Now
        };
        await _db.SendMessage(message); //---------------------- dbService
    }

    public async Task AddPhoto(long chatId, string path, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chatId);
        if (user == null)
            return;

        Photos photo = new()
        {
            User_ID = user.User_ID,
            Path = path
        };
        await dbService.AddPhoto(photo);
    }

    public async Task<string?> GetPhotoPath(int user_id, DbService dbService)
    {
        return await dbService.GetPhotoPath(user_id);
    }

    public async Task<string?> GetPhotoPath(long chatId, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chatId);
        if (user == null)
            return null;

        return await dbService.GetPhotoPath(user.User_ID);
    }

    public async Task HandleCommand(string command, string[] args, Message message, ITelegramBotClient botClient, DbService dbService, string username)
    {
        switch (command)
        {
            case "/start":
                await dbService.AddUser(message.Chat.Id, username);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет! Я создан для того, чтобы ребята из разных институтов УдГУ могли познакомиться друг с другом.",
                    cancellationToken: default);
                break;
            case "/help":
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я умею:\n" +
                          "/help - показать это сообщение\n" +
                          "/start - начать общение с ботом\n" +
                          "/mylikes - показать количество лайков, которые вы получили\n",
                    cancellationToken: default);
                break;
            case "/block":
                if (username != "mosinisom")
                    break;

                await BlockUser(args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы заблокировали пользователя с ником " + args[0],
                    cancellationToken: default);
                break;
            case "/unblock":
                if (username != "mosinisom")
                    break;

                await UnblockUser(args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы разблокировали пользователя с ником " + args[0],
                    cancellationToken: default);
                break;
            case "/mylikes":
                int likesCount = await dbService.GetLikesCount(message.Chat.Id);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Количество лайков: " + likesCount,
                    cancellationToken: default);
                break;

            default:
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понял тебя :(",
                    cancellationToken: default);
                break;
        }
    }
}