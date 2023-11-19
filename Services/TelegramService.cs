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
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<DbService>();

            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var chatId = callbackQuery.Message.Chat.Id;
                var messageId = callbackQuery.Message.MessageId;
                var callbackData = callbackQuery.Data;

                if (callbackData.StartsWith("/"))
                {
                    var command = callbackData.Split(" ")[0];
                    var args = callbackData.Split(" ")[1..];
                    await _appLogic.HandleCommand(command, args, callbackQuery.Message, botClient, dbService, callbackQuery.From.Username);
                }
                await botClient.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup: null);
            }
            else if (update.Message != null)
            {
                var message = update.Message;
                var chatId = message.Chat.Id;
                var username = message.From.Username;


                // if message is a photo
                if (message.Photo != null)
                {
                    var photo = message.Photo[^1];
                    await _appLogic.AddPhoto(chatId, photo.FileId, dbService);

                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: InputFile.FromFileId(_appLogic.GetPhotoPath(chatId, dbService).Result),
                        caption: $"{username}, это Ваше фото профиля! Если не нравится, пришлите другое :)",
                        cancellationToken: cancellationToken);

                    StateOfBot state = await _appLogic.GetState(chatId, dbService);
                    if (state.State == (int)stateEnum.waiting_for_student_photo)
                    {
                        await _appLogic.HandleState(state, message, botClient, dbService);
                    }

                }
                else if (message.Text is { } messageText) // check if message is text
                {
                    StateOfBot state = await _appLogic.GetState(chatId, dbService);
                    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                    if (messageText.StartsWith("/"))
                    {
                        var command = messageText.Split(" ")[0];
                        var args = messageText.Split(" ")[1..];
                        await _appLogic.HandleCommand(command, args, message, botClient, dbService, username);
                    }
                    else if (state.State != 0)
                    {
                        await _appLogic.HandleState(state, message, botClient, dbService);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Я не понял тебя сейчас :(",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка в TelegramService.HandleUpdateAsync: {ex.Message}");
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