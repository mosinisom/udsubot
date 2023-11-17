using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class TelegramService
{
    IServiceScopeFactory _scopeFactory;
    IConfiguration _cfg;
    ILogger<TelegramService> _log;
    TelegramBotClient _bot;
    CancellationTokenSource _cts;
    AppLogicService _appLogic;

    public TelegramService(IServiceScopeFactory scopeFactory, IConfiguration cfg, ILogger<TelegramService> log, AppLogicService appLogic)
    {
        _scopeFactory = scopeFactory;
        _cfg = cfg;
        _log = log;
        _bot = new(_cfg.GetValue<string>("BotSecret"));
        _cts = new();
        _appLogic = appLogic;
    }

    public void Start()
    {
        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbService = scope.ServiceProvider.GetRequiredService<DbService>();

        if (update.Message != null)
        {
            var message = update.Message;
            var chatId = message.Chat.Id;
            var username = message.From.Username;

            // if message is a photo
            if (message.Photo != null)
            {
                var photo = message.Photo[^1];
                await _appLogic.AddPhoto(chatId, photo.FileId, dbService);

                // send that photo back with caption "Photo ID: {photoId}"
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromFileId(_appLogic.GetPhotoPath(chatId, dbService).Result),
                    caption: $"Photo ID: {photo.FileId} from {username}",
                    cancellationToken: cancellationToken);
            }
            else if (message.Text is { } messageText) // check if message is text
            {
                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                if (messageText.StartsWith("/"))
                {
                    var command = messageText.Split(" ")[0];
                    var args = messageText.Split(" ")[1..];
                    await _appLogic.HandleCommand(command, args, message, botClient, dbService, username);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Я не понял тебя :(",
                        cancellationToken: cancellationToken);
                }
            }
        }
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

}