using System.Reflection.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class AppLogicService
{
    DbService _db;
    ILogger<AppLogicService> _log;

    public AppLogicService(DbService db, ILogger<AppLogicService> log)
    {
        _db = db;
        _log = log;
    }

    readonly Dictionary<int, string> InstituteDictionary = new()
    {
        { 0, ""},
        { 1, "ИГЗ" },
        { 2, "ИИиД" },
        { 3, "ИИиС" },
        { 4, "ИМИТиФ" },
        { 5, "ИНиГ" },
        { 6, "ИЕН" },
        { 7, "ИППСТ" },
        { 8, "ИПСУБ" },
        { 9, "ИСК" },
        { 10, "ИУФФУиЖ" },
        { 11, "ИФКиС" },
        { 12, "ИЭиУ" },
        { 13, "ИЯЛ" },
        { 14, "МКПО" }
    };

    public async Task SendRandomProfile(long chatId, string fromUsername, ITelegramBotClient botClient, DbService dbService)
    {
        try
        {
            _log.LogInformation("User {Username} requested random profile in chat {ChatId}", fromUsername, chatId);
            Students? student = await dbService.GetOneRandomStudent(fromUsername);
            if (student == null)
                return;

            InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("❤️", "/likehim " + student.TelegramLink),
                    InlineKeyboardButton.WithCallbackData("✉️", "/message " + student.TelegramLink)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Жалоба", "/complain " + student.TelegramLink),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
                }
            });

            var photoPath = await GetPhotoPath(student.TelegramLink, dbService);
            if (photoPath != null)
            {
                var inputFile = InputFile.FromFileId(photoPath);
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: inputFile,
                    caption: $"{student.Name}, {student.Year} курс {InstituteDictionary[student.Institute_ID]}\n{student.Description}\nКоличество лайков: {await dbService.GetLikesCount(student.TelegramLink)}",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"{student.Name}, {student.Year} курс {InstituteDictionary[student.Institute_ID]}\n{student.Description}\nКоличество лайков: {await dbService.GetLikesCount(student.TelegramLink)}",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
            }
        }
        catch (Exception e)
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
                }
            });
            _log.LogError(e.Message, "Error while sending random profile to user {Username} in chat {ChatId}", fromUsername, chatId);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Произошла ошибка, попробуйте ещё раз!",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: default);
        }
    }

    public async Task SendProfile(long chatId, string whoUsername, ITelegramBotClient botClient, DbService dbService)
    {
        Students? student = await dbService.GetStudentByTelegramLink(whoUsername);
        if (student == null)
            return;

        InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Жалоба", "/complain " + student.TelegramLink),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
            }
        });

        try
        {
            var photoPath = await GetPhotoPath(student.TelegramLink, dbService);
            if (photoPath != null)
            {
                var inputFile = InputFile.FromFileId(photoPath);
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: inputFile,
                    caption: $"{student.Name}, {student.Year} курс {InstituteDictionary[student.Institute_ID]}\n{student.Description}\nКоличество лайков: {await dbService.GetLikesCount(student.TelegramLink)}",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"{student.Name}, {student.Year} курс {InstituteDictionary[student.Institute_ID]}\n{student.Description}\nКоличество лайков: {await dbService.GetLikesCount(student.TelegramLink)}",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error when sending message for user {Username}", chatId);
        }
    }

    public async Task SendProfile(string fromUsername, string whoUsername, ITelegramBotClient botClient, DbService dbService)
    {
        Users? user = await dbService.GetUserByTelegramLink(fromUsername);

        Students? studentWho = await dbService.GetStudentByTelegramLink(whoUsername);
        if (studentWho == null)
            return;

        InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Жалоба", "/complain " + studentWho.TelegramLink),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
            }
        });

        try
        {
            var photoPath = await GetPhotoPath(studentWho.TelegramLink, dbService);
            if (photoPath != null)
            {
                var inputFile = InputFile.FromFileId(photoPath);
                if (inputFile != null)
                {
                    await botClient.SendPhotoAsync(
                        chatId: user.Chat_ID,
                        photo: inputFile,
                        caption: $"{studentWho.Name}, {studentWho.Year} курс {InstituteDictionary[studentWho.Institute_ID]}\n{studentWho.Description}\nКоличество лайков: {await dbService.GetLikesCount(studentWho.TelegramLink)}",
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: default);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: user.Chat_ID,
                    text: $"{studentWho.Name}, {studentWho.Year} курс {InstituteDictionary[studentWho.Institute_ID]}\n{studentWho.Description}\nКоличество лайков: {await dbService.GetLikesCount(studentWho.TelegramLink)}",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error when sending message for user {Username}", user.Chat_ID);
        }
    }


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

    public async Task BanUser(long chat_id, string reason, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chat_id);
        if (user == null)
            return;

        Students? student = await dbService.GetStudentByChatId(chat_id);
        if (student == null)
            return;

        BannedUsers bannedUser = new()
        {
            Chat_id = user.Chat_ID,
            Username = student.TelegramLink,
            Reason = reason
        };
        await dbService.BanUser(bannedUser);
    }

    public async Task BanUser(string telegramlink, string reason, DbService dbService)
    {
        Users? user = await dbService.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;

        Students? student = await dbService.GetStudentByTelegramLink(telegramlink);
        if (student == null)
            return;

        BannedUsers bannedUser = new()
        {
            Chat_id = user.Chat_ID,
            Username = student.TelegramLink,
            Reason = reason
        };
        await dbService.BanUser(bannedUser);
    }

    public async Task UnbanUser(string telegramlink, DbService dbService)
    {
        Users? user = await dbService.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return;

        Students? student = await dbService.GetStudentByTelegramLink(telegramlink);
        if (student == null)
            return;

        BannedUsers bannedUser = new()
        {
            Chat_id = user.Chat_ID,
            Username = student.TelegramLink,
            Reason = ""
        };
        await dbService.UnbanUser(bannedUser);
    }

    public async Task<bool> IsUserBanned(string username, DbService dbService)
    {
        return await dbService.IsUserBanned(username);
    }

    public async Task AddStudentCardNumber(long chat_id, long student_card_number, DbService dbService)
    {
        Users? user = await dbService.GetUserByChatId(chat_id);
        if (user == null)
            return;
        user.StudentCardNumber = student_card_number;
        if (student_card_number.ToString().Length == 10)
            user.HasAccess = true;
        await dbService.UpdateUser(user);
    }

    public async Task AddStudentName(long chat_id, string name, DbService dbService)
    {
        Students? student = await dbService.GetStudentByChatId(chat_id);
        if (student == null)
            return;

        student.Name = name;
        await dbService.UpdateStudent(student);
    }

    public async Task AddStudentYear(long chat_id, int year, DbService dbService)
    {
        Students? student = await dbService.GetStudentByChatId(chat_id);
        if (student == null)
        {
            _log.LogInformation("User can't add his year");
            return;
        }

        student.Year = year;
        await dbService.UpdateStudent(student);
    }

    public async Task AddStudentDescription(long chat_id, string description, DbService dbService)
    {
        Students? student = await dbService.GetStudentByChatId(chat_id);
        if (student == null)
            return;

        student.Description = description;
        await dbService.UpdateStudent(student);
    }

    public async Task AddStudentInstitute(long chat_id, int institute_id, DbService dbService)
    {
        Students? student = await dbService.GetStudentByChatId(chat_id);
        if (student == null)
        {
            _log.LogInformation("User can't add his institute in chat {ChatId}", chat_id);
            return;
        }

        student.Institute_ID = institute_id;
        await dbService.UpdateStudent(student);
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

    public async Task<string?> GetPhotoPath(string telegramlink, DbService dbService)
    {
        Users? user = await dbService.GetUserByTelegramLink(telegramlink);
        if (user == null)
            return null;

        return await dbService.GetPhotoPath(user.User_ID);
    }

    public async Task SendMessageByUsername(string toUsername, string message, ITelegramBotClient botClient, DbService dbService, IReplyMarkup? replyMarkup = null)
    {
        Users? user = await dbService.GetUserByTelegramLink(toUsername);
        if (user == null)
            return;

        await botClient.SendTextMessageAsync(
            chatId: user.Chat_ID,
            text: message,
            replyMarkup: replyMarkup,
            cancellationToken: default);
    }

    public async Task SendMessageToAll(string message, ITelegramBotClient botClient, DbService dbService, IReplyMarkup? replyMarkup = null)
    {
        List<Users> users = await dbService.GetAllUsers();
        foreach (Users user in users)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Chat_ID,
                text: message,
                replyMarkup: replyMarkup,
                cancellationToken: default);
        }
    }

    public async Task Like(long chatId, string username, DbService dbService, ITelegramBotClient botClient)
    {
        Students? student1 = await dbService.GetStudentByChatId(chatId);
        if (student1 == null)
            return;

        Students? student2 = await dbService.GetStudentByTelegramLink(username);
        if (student2 == null)
            return;

        Likes like = new()
        {
            FromStudent_ID = student1.Student_ID,
            ToStudent_ID = student2.Student_ID,
            Date = DateTime.Now
        };

        await dbService.LikeStudent(like);
        await SendMessageByUsername(username, "❤️ Вам пришёл лайк от @" + student1.TelegramLink + "!", botClient, dbService);
        await SendProfile(username, student1.TelegramLink, botClient, dbService);
    }

    public async Task<StateOfBot> GetState(long chatId, DbService dbService)
    {
        _log.LogInformation("User {Username} got state", chatId);
        return await dbService.GetStateOfBot(chatId);
    }

    public async Task SetState(long chatId, int state, string? data, DbService dbService)
    {
        _log.LogInformation("User {Username} set state", chatId);
        await dbService.SetStateOfBot(chatId, state, data);
    }

    public async Task<int> GetCountOfUsers(DbService dbService)
    {
        return await dbService.GetCountOfUsers();
    }

    public async Task HandleCommand(string command, string[] args, Message message, ITelegramBotClient botClient, DbService dbService, string username)
    {
        switch (command)
        {
            case "/start":
                if (username == null)
                {
                    await SetState(message.Chat.Id, (int)stateEnum.user_does_not_have_username, "", dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "У Вас не установлен никнейм. Пожалуйста, установите его в настройках телеграма и нажмите /start",
                        cancellationToken: default);
                    break;
                }
                _log.LogInformation("User {Username} started bot", username);
                await dbService.AddUser(message.Chat.Id, username);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет! Я создан для того, чтобы ребята из разных институтов УдГУ могли познакомиться друг с другом. \n" +
                          "Ты сейчас создашь такую же анкету (только о себе): ",
                    cancellationToken: default);

                await Task.Delay(1750);

                try
                {
                    var photoPath = await GetPhotoPath("mosinisom", dbService);
                    if (photoPath != null)
                    {
                        var inputFile = InputFile.FromFileId(photoPath);
                        if (inputFile != null)
                        {
                            await botClient.SendPhotoAsync(
                                chatId: message.Chat.Id,
                                photo: inputFile,
                                caption: "Саша Мосин, 3 курс ИМИТиФ, Прикладная информатика в юриспруденции. \n" +
                                         "Мои основные корпуса: 4 и 6 \n" +
                                         "Хочу подружиться с людьми из каждого института! Почти всегда меня можно встретить с улыбкой на лице)" + "\n" +
                                         "А ещё я очень люблю обниматься! \n" +
                                         "Количество лайков: " + await dbService.GetLikesCount("mosinisom"),
                                cancellationToken: default);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Саша Мосин, 3 курс ИМИТиФ, Прикладная информатика в юриспруденции. \n" +
                                     "Мои основные корпуса: 4 и 6 \n" +
                                     "Хочу подружиться с людьми из каждого института! Почти всегда меня можно встретить с улыбкой на лице)" + "\n" +
                                     "А ещё я очень люблю обниматься! \n" +
                                     "Количество лайков: " + await dbService.GetLikesCount("mosinisom") + "\n" + "\n" +
                                        "ЗДЕСЬ ДОЛЖНО БЫТЬ ФОТО, НО ПОЧЕМУ-ТО ПУСТО :( \n",
                            cancellationToken: default);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error when sending message for user {Username}", message.Chat.Id);
                }


                var user = await dbService.GetUserByChatId(message.Chat.Id);
                await Task.Delay(1000);

                if (user.HasAccess == false)
                {
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_number, "", dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Введите номер студенческого билета (тот, что под штрих кодом), пожалуйста, \n" +
                              " чтобы я мог проверить, студент ли ты УдГУ:",
                        cancellationToken: default);
                }
                break;
            case "/help":
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я умею:\n" +
                          "/help - показать это сообщение\n" +
                          "/start - заполнить анкету заново (фото профиля изменить можно в любой момент, просто отправив его в чат)\n" +
                          "/mylikes - показать количество лайков, которые вы получили\n" +
                          "/myprofile - показать вашу анкету\n" +
                          "/complain ник_пользователя - отправить жалобу на пользователя\n" +
                          "/next - показать другие анкеты\n",
                    cancellationToken: default);
                _log.LogInformation("User {Username} got help message", username);
                break;
            case "/block":
                if (username != "mosinisom")
                    break;

                await BlockUser(args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы заблокировали пользователя с ником " + args[0],
                    cancellationToken: default);
                _log.LogInformation("User {Username} blocked {Username2}", username, args[0]);
                break;
            case "/unblock":
                if (username != "mosinisom")
                    break;

                await UnblockUser(args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы разблокировали пользователя с ником " + args[0],
                    cancellationToken: default);
                _log.LogInformation("User {Username} unblocked {Username2}", username, args[0]);
                break;
            case "/ban":
                if (username != "mosinisom")
                    break;
                _log.LogInformation("User {Username} banned {Username2}", username, args[0]);

                string reason = "";
                for (int i = 1; i < args.Length; i++)
                {
                    reason += args[i] + " ";
                }

                await BanUser(args[0], reason, dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы забанили пользователя с ником " + args[0],
                    cancellationToken: default);
                break;
            case "/unban":
                if (username != "mosinisom")
                    break;
                _log.LogInformation("User {Username} unbanned {Username2}", username, args[0]);

                await UnbanUser(args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы разбанили пользователя с ником " + args[0],
                    cancellationToken: default);
                break;
            case "/mylikes":
                int likesCount = await dbService.GetLikesCount(message.Chat.Id);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Количество лайков: " + likesCount,
                    cancellationToken: default);
                break;
            case "/likehim":
                _log.LogInformation("User {Username} liked {Username2}", username, args[0]);
                if (username == args[0])
                    return;
                await Like(message.Chat.Id, args[0], dbService, botClient);
                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
                    }
                });
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы лайкнули пользователя",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                break;
            case "/myprofile":
                Students? student = await dbService.GetStudentByChatId(message.Chat.Id);
                if (student == null)
                    return;
                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromFileId(GetPhotoPath(message.Chat.Id, dbService).Result),
                    caption: "Ваша анкета: \n"
                            + student.Name + ", "
                            + student.Year + " курс "
                            + InstituteDictionary[student.Institute_ID] + "\n"
                            + student.Description
                            + "\nКоличество лайков: " + await dbService.GetLikesCount(message.Chat.Id),
                    cancellationToken: default);
                break;
            case "/setinstitute":
                if (int.TryParse(args[0], out int institute_id))
                {
                    _log.LogInformation("Выбор института {Institute} пользователем {Username}", InstituteDictionary[institute_id], username);
                    if (institute_id < 1 || institute_id > 14)
                        return;
                    await AddStudentInstitute(message.Chat.Id, institute_id, dbService);
                }
                break;
            case "/message":
                if (username == args[0])
                    return;
                StateOfBot state = await GetState(message.Chat.Id, dbService);
                if (state.State != (int)stateEnum.default_state)
                    return;
                await SetState(message.Chat.Id, (int)stateEnum.waiting_for_message_to_another_student, args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Введите сообщение для выбранного пользователя:",
                    cancellationToken: default);
                break;
            case "/next":
                state = await GetState(message.Chat.Id, dbService);
                if (state.State != (int)stateEnum.default_state)
                    return;
                await SendRandomProfile(message.Chat.Id, username, botClient, dbService);
                break;
            case "/sendtoall":
                if (username != "mosinisom")
                    break;

                string messageToSend = "";
                for (int i = 0; i < args.Length; i++)
                {
                    messageToSend += args[i] + " ";
                }

                await SendMessageToAll(messageToSend, botClient, dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Сообщение всем отправлено!",
                    cancellationToken: default);
                break;
            case "/profile":
                await SendProfile(message.Chat.Id, args[0], botClient, dbService);
                break;
            case "/complain":
                if(args.Length == 0)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вы не указали пользователя, на которого хотите пожаловаться!",
                        cancellationToken: default);
                    return;
                }

                _log.LogInformation("User {Username} reported {Username2}", username, args[0]);
                if (username == args[0])
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Ну зачем жаловаться на самого себя? :С",
                        cancellationToken: default);
                    return;
                }

                if (args[0] == "mosinisom")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вы не можете пожаловаться на Сашу Мосина! Если Вам от него что-то нужно, просто напишите ему сообщение :)",
                        cancellationToken: default);
                    return;
                }

                inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Другие анкеты", "/next"),
                    }
                });
                await SendMessageByUsername("mosinisom", "Жалоба на пользователя @" + args[0] + ":\n от @" + username, botClient, dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Жалоба отправлена!",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                break;
            case "/getcount":
                int count = await GetCountOfUsers(dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Количество пользователей: " + count,
                    cancellationToken: default);
                break;
            default:
                inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("анкеты", "/next"),
                    }
                });
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понял тебя :(",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                break;
        }
    }

    public async Task HandleState(StateOfBot state, Message message, ITelegramBotClient botClient, DbService dbService)
    {
        switch (state.State)
        {
            case (int)stateEnum.user_does_not_have_username:
                if (message.Text is { } messageText)
                {
                    if (message.Chat.Username != null)
                        await SetState(message.Chat.Id, (int)stateEnum.default_state, "", dbService);

                    if (messageText.StartsWith("/"))
                    {
                        var command = messageText.Split(" ")[0];
                        var args = messageText.Split(" ")[1..];
                        await HandleCommand(command, args, message, botClient, dbService, message.Chat.Username);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "У Вас не установлен никнейм. Пожалуйста, установите его в настройках телеграма и нажмите /start",
                            cancellationToken: default);
                    }
                }
                break;
            case (int)stateEnum.waiting_for_student_number:
                if (!long.TryParse(message.Text, out long student_card_number))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Номер студенческого билета должен состоять только из цифр. Попробуйте еще раз:",
                        cancellationToken: default);
                    break;
                }
                if (message.Text.Length == 10)
                {
                    await AddStudentCardNumber(message.Chat.Id, student_card_number, dbService);
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_name, "", dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Введите Ваше имя и фамилию:",
                        cancellationToken: default);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Номер студенческого билета не подошёл, попробуйте ещё раз:",
                        cancellationToken: default);
                }
                break;

            case (int)stateEnum.waiting_for_student_name:
                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИГЗ", "/setinstitute 1")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИИиД", "/setinstitute 2")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИИиС", "/setinstitute 3")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИМИТиФ", "/setinstitute 4")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИНиГ", "/setinstitute 5")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИЕН", "/setinstitute 6")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИППСТ", "/setinstitute 7")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИПСУБ", "/setinstitute 8")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИСК", "/setinstitute 9")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИУФФУиЖ", "/setinstitute 10")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИФКиС", "/setinstitute 11")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИЭиУ", "/setinstitute 12")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ИЯЛ", "/setinstitute 13")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("МКПО", "/setinstitute 14")
                    }
                });
                await AddStudentName(message.Chat.Id, message.Text, dbService);
                await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_year, "", dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Введите Ваш курс и выберите институт:",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                break;

            case (int)stateEnum.waiting_for_student_year:
                if (!int.TryParse(message.Text, out int year))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Курс должен состоять только из цифр. Попробуйте еще раз:",
                        cancellationToken: default);
                    break;
                }
                if (year >= 1 && year <= 6)
                {
                    await AddStudentYear(message.Chat.Id, year, dbService);
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_description, "", dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Введите текст анкеты:",
                        cancellationToken: default);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Курс должен быть от 1 до 6 (есть шанс, что у вас не так). Попробуйте еще раз:",
                        cancellationToken: default);
                }
                break;

            case (int)stateEnum.waiting_for_student_description:
                await AddStudentDescription(message.Chat.Id, message.Text, dbService);
                await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_photo, "", dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Пришлите фото для анкеты:" + "\n" +
                          "Просьба использовать реальное фото, где видно Ваше лицо, иначе теряется смысл Вашей анкеты.",
                    cancellationToken: default);
                break;

            case (int)stateEnum.waiting_for_student_photo:
                await SetState(message.Chat.Id, (int)stateEnum.default_state, "", dbService);
                Students? student = await dbService.GetStudentByChatId(message.Chat.Id);
                if (student == null)
                    return;

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Спасибо за заполнение анкеты! Вы всегда можете полюбоваться ей через \"/myprofile\", заполнить заново командой \"/start\". \n"
                        + "Список команд \"/help\"."
                        + "\n\n А если захотите закрыть свою анкету, напишите Саше Мосину (квест еще тот, если у Вас нет его контактов)",
                    cancellationToken: default);

                inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Смотреть анкеты", "/next"),
                    }
                });

                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromFileId(GetPhotoPath(message.Chat.Id, dbService).Result),
                    caption:
                        "Ваша анкета: \n" +
                            "Имя: " + student.Name + "\n" +
                            "Курс: " + student.Year + "\n" +
                            "Институт: " + InstituteDictionary[student.Institute_ID] + "\n" +
                            "Текст: " + student.Description,
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                _log.LogInformation("User {Username} filled his profile", message.Chat.Username);
                break;

            case (int)stateEnum.waiting_for_message_to_another_student:

                await SendMessageByUsername(state.Data, message.Text + "\n(✉️ от @" + message.Chat.Username + ")", botClient, dbService);
                await SendProfile(state.Data, message.Chat.Username, botClient, dbService);
                _log.LogInformation("User {Username} sent message to {Username2}", message.Chat.Username, state.Data);
                await SetState(message.Chat.Id, (int)stateEnum.default_state, "", dbService);

                inlineKeyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Смотреть дальше", "/next"),
                    }
                });

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Сообщение отправлено (наверно)!"
                        + "\n\n"
                        + "Если хотите посмотреть другие анкеты, нажмите на кнопку ниже:",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понял тебя тут :(",
                    cancellationToken: default);
                break;

        }
    }
}