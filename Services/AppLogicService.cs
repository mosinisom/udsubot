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
            return;

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
            return;

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



    public async Task Like(long chatId, string username, DbService dbService)
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
    }

    public async Task Like(long chatId, int student_id, DbService dbService)
    {
        Students? student1 = await dbService.GetStudentByChatId(chatId);
        if (student1 == null)
            return;

        Likes like = new()
        {
            FromStudent_ID = student1.Student_ID,
            ToStudent_ID = student_id,
            Date = DateTime.Now
        };
        await dbService.LikeStudent(like);
    }

    public async Task<int> GetState(long chatId, DbService dbService)
    {
        return await dbService.GetStateOfBot(chatId);
    }

    public async Task SetState(long chatId, int state, DbService dbService)
    {
        await dbService.SetStateOfBot(chatId, state);
    }

    public async Task HandleCommand(string command, string[] args, Message message, ITelegramBotClient botClient, DbService dbService, string username)
    {
        switch (command)
        {
            case "/start":
                await dbService.AddUser(message.Chat.Id, username);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет! Я создан для того, чтобы ребята из разных институтов УдГУ могли познакомиться друг с другом. \n" +
                          "Ты сейчас создашь такую же анкету (только о себе): ",
                    cancellationToken: default);

                await Task.Delay(1750);

                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]
                {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Like", "/like mosinisom"),
                        }
                    });

                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromFileId(GetPhotoPath("mosinisom", dbService).Result),
                    caption: "Саша Мосин, 3 курс ИМИТиФ, Прикладная информатика в юриспруденции. \n" +
                             "Мои основные корпуса: 4 и 6 \n" +
                             "Люблю знакомиться с новыми людьми и обниматься. Почти всегда меня можно встретить с улыбкой на лице)",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);

                var user = await dbService.GetUserByChatId(message.Chat.Id);
                await Task.Delay(1000);

                if (user.HasAccess == false)
                {
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_number, dbService);
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
            case "/like":
                await Like(message.Chat.Id, args[0], dbService);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы лайкнули пользователя",
                    cancellationToken: default);
                break;
            case "/myprofile":
                Students? student = await dbService.GetStudentByChatId(message.Chat.Id);
                if (student == null)
                    return;
                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromFileId(GetPhotoPath(message.Chat.Id, dbService).Result),
                    caption: "Ваша анкета: \n" +
                             student.Name + ", " + student.Year +" курс "+ InstituteDictionary[student.Institute_ID] +"\n" +
                             student.Description,
                    cancellationToken: default);
                break;
            case "/setinstitute":
                if (!int.TryParse(args[0], out int institute_id))
                {
                    if(1 > institute_id || institute_id > 14)
                        return;
                    await AddStudentInstitute(message.Chat.Id, institute_id, dbService);
                }

                break;

            default:
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понял тебя :(",
                    cancellationToken: default);
                break;
        }
    }

    // await _appLogic.HandleState(state, message, botClient, dbService);
    public async Task HandleState(int state, Message message, ITelegramBotClient botClient, DbService dbService)
    {
        switch (state)
        {
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
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_name, dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Введите Ваше имя и фамилию:",
                        cancellationToken: default);
                } else
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
                await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_year, dbService);
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
                    await SetState(message.Chat.Id, (int)stateEnum.waiting_for_student_description, dbService);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Введите текст анкеты:",
                        cancellationToken: default);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Курс должен быть от 1 до 6. Попробуйте еще раз:",
                        cancellationToken: default);
                }
                break;
            case (int)stateEnum.waiting_for_student_description:
                Students? student = await dbService.GetStudentByChatId(message.Chat.Id);
                if (student == null)
                    return;

                await AddStudentDescription(message.Chat.Id, message.Text, dbService);

                await SetState(message.Chat.Id, (int)stateEnum.default_state, dbService);
                inlineKeyboardMarkup = new(new[]
                {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Моя анкета", "/myprofile"),
                        }
                    });
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Спасибо за заполнение анкеты! Теперь вы можете посмотреть её, нажав на кнопку ниже:",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: default);


                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromFileId(GetPhotoPath(message.Chat.Id, dbService).Result),
                    caption: "Ваша анкета: \n" +
                             "Имя: " + student.Name + "\n" +
                             "Курс: " + student.Year + "\n" +
                             "Текст: " + student.Description,
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