using MediatR;
using PiVPNManager.Application.Clients.Commands.CreateClient;
using PiVPNManager.Application.Clients.Commands.DeleteClient;
using PiVPNManager.Application.Clients.Queries.GetClient;
using PiVPNManager.Application.Clients.Queries.GetClientConfFile;
using PiVPNManager.Application.Clients.Queries.GetClientQrCode;
using PiVPNManager.Application.Clients.Queries.GetClients;
using PiVPNManager.Application.Clients.Queries.GetClientStats;
using PiVPNManager.Application.Clients.Queries.GetUserCanCreateClient;
using PiVPNManager.Application.Common.Exceptions;
using PiVPNManager.Application.Servers.Queries.GetServerByName;
using PiVPNManager.Application.Servers.Queries.GetServers;
using SixLabors.ImageSharp.Formats.Png;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace PiVPNManager.Infrastructure.Bot.Handlers
{
    public sealed class UpdateHandlers : IUpdateHandlers
    {
        private const int max_clients = 2;
        private readonly ISender _mediator;
        private readonly UsersActionsManagerService _usersActionsManager;

        public UpdateHandlers(ISender mediator, UsersActionsManagerService usersActionsManager)
        {
            _mediator = mediator;
            _usersActionsManager = usersActionsManager;
        }

        public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, Dictionary<long, UserStates> usersStates, Dictionary<long, UserClient> userClients)
        {
            if (message.Type != MessageType.Text)
                return;

            Message respMsg = null;
            var chatId = message.Chat.Id;
            var userState = UserStates.None;

            if (usersStates.ContainsKey(chatId))
                userState = usersStates[chatId];

            if (userState != UserStates.None)
            {
                respMsg = await SendCreateClient(botClient, message, userState);
            }
            else
            {
                var action = message.Text!.Split(' ')[0] switch
                {
                    "/start" => SendStartMessage(botClient, message),
                    "/help" => SendStartMessage(botClient, message),
                    "/servers" => SendServersList(botClient, message),
                    "/my_clients" => SendClientsList(botClient, message),
                    "/add_client" => SendCreateClient(botClient, message, userState),
                    _ => SendStartMessage(botClient, message)
                };

                respMsg = await action;
            }

            async Task<Message> SendCreateClient(ITelegramBotClient bot, Message message, UserStates userState)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                IReplyMarkup keyboard = new ReplyKeyboardRemove();
                var msgText = new StringBuilder();
                var chatId = message.Chat.Id;

                if (userState == UserStates.None)
                {
                    var canCreateClient = await _mediator.Send(new GetUserCanCreateClientQuery
                    {
                        UserId = chatId
                    });
                    
                    if (canCreateClient)
                    {
                        usersStates[chatId] = UserStates.EnterClientName;
                        msgText.AppendLine("Введите название для клиента, например, *телефон* или *ноутбук* или что угодно, это нужно только для вашего удобства\\.");
                        msgText.AppendLine("❗️Название должно быть от 2 до 8 символов\\!");
                    }
                    else
                    {
                        msgText.AppendLine("У вас уже есть максимально допустимое количество клиентов\\.");
                    }
                }                

                if (userState == UserStates.EnterClientName)
                {
                    var userInput = message.Text;

                    if (string.IsNullOrWhiteSpace(userInput) || userInput.Length < 2 || userInput.Length > 8)
                    {
                        msgText.AppendLine("Некорректное название клиента, попробуйте еще раз");
                    }
                    else
                    {
                        _usersActionsManager.AddClientName(chatId, userInput);

                        msgText.AppendLine("Выберите место расположения сервера:");

                        var buttonRows = new List<List<KeyboardButton>>();
                        var serversResult = await _mediator.Send(new GetServersQuery
                        {
                            NotDead = true,
                            AvailableOnly = true
                        });

                        if (serversResult.IsError)
                        {
                            msgText.AppendLine("Во время загрузки списка серверов произошла ошибка:");
                            foreach (var error in serversResult.Errors)
                            {
                                msgText.AppendLine(error.Message.ToEscapeMarkDown());
                            }
                        }
                        else
                        {
                            foreach (var server in serversResult.Payload)
                            {
                                buttonRows.Add(
                                    new List<KeyboardButton>
                                    {
                                new KeyboardButton(server.Name)
                                    });
                            };

                            keyboard = new ReplyKeyboardMarkup(buttonRows);
                            usersStates[chatId] = UserStates.ChooseServer;
                        }
                    }
                }

                if (userState == UserStates.ChooseServer)
                {
                    try
                    {
                        var serverResult = await _mediator.Send(new GetServerByNameQuery 
                        { 
                            ServerName = message.Text 
                        });

                        if (serverResult.IsError)
                        {
                            var msgBuilder = new StringBuilder();
                            serverResult.Errors.ForEach(e => msgBuilder.AppendLine(e.Message.ToEscapeMarkDown()));
                            throw new Exception(msgBuilder.ToString());
                        }

                        _usersActionsManager.AddClientServer(chatId, serverResult.Payload.Id);

                        var userClient = _usersActionsManager.GetUserClient(chatId);

                        var clientResult = await _mediator.Send(new CreateClientCommand
                        {
                            UserId = chatId,
                            ServerId = userClient.ServerId,
                            ClientName = userClient.ClientName
                        });

                        if (clientResult.IsError)
                        {
                            msgText.AppendLine("Попытка создать клиент не удалась:");
                            foreach (var error in clientResult.Errors)
                            {
                                msgText.AppendLine(error.Message.ToEscapeMarkDown());
                            }
                        }
                        else
                        {
                            msgText.AppendLine($"Клиент *{clientResult.Payload.Name.ToEscapeMarkDown()}* успешно создан\\!");
                            msgText.AppendLine();
                            msgText.AppendLine($"Используйте команду /my_clients для просмотра ваших клиентов.".ToEscapeMarkDown());
                            _usersActionsManager.RemoveUserClient(chatId);                         
                            usersStates[chatId] = UserStates.None;
                        }
                    }
                    catch (NotFoundException)
                    {
                        msgText.AppendLine("Не удалось найти указанный сервер, попробуйте еще раз.");
                    }
                    catch (Exception ex)
                    {
                        msgText.AppendLine("Попытка создать клиент не удалась: " + ex.Message);
                    }
                }

                return await bot.SendTextMessageAsync(
                        chatId: chatId,
                        text: msgText.ToString(),
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: keyboard);
            }

            async Task<Message> SendStartMessage(ITelegramBotClient botClient, Message message)
            {
                var msg = new StringBuilder();
                msg.AppendLine($"Бот представляет из себя дополнение к приложению WireGuard\\.");
                msg.AppendLine($"[На официальной странице]({"https://www.wireguard.com/install/".ToEscapeMarkDown()}) можете скачать приложение для вашего устройства, а бот позволит создать клиента для него\\.");
                
                msg.AppendLine();
                msg.AppendLine("Создать клиент можно с помощью команды /add_client.".ToEscapeMarkDown());
                msg.AppendLine("Затем для каждого созданного вами клиента вы cможете получить файл конфигурации или qr-код, для создания тоннеля в WireGuard.".ToEscapeMarkDown());
                msg.AppendLine();
                msg.AppendLine($"ℹ️ Максимальное число клиентов: {max_clients}".ToEscapeMarkDown());
                msg.AppendLine();
                msg.AppendLine("Можно посмотреть список серверов по команде /servers. В этом списке так же будет отражен статус доступности серверов.".ToEscapeMarkDown());
                msg.AppendLine("ℹ️ Если какой-то сервер становится недоступен, то вам придет уведомление об этом. Так же придет уведомление, когда сервер снова вернется в строй.".ToEscapeMarkDown());

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: msg.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: new ReplyKeyboardRemove());
            }

            async Task<Message> SendServersList(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                var chatId = message.Chat.Id;
                var msg = new StringBuilder();
                
                var serversResult = await _mediator.Send(new GetServersQuery
                {
                    NotDead = true,
                    AvailableOnly = false
                });

                if (serversResult.IsError)
                {
                    msg.AppendLine("Во время загрузки списка серверов произошла ошибка:");
                    serversResult.Errors.ForEach(e => msg.AppendLine(e.Message.ToEscapeMarkDown()));

                }
                else
                {
                    msg.AppendLine("Список серверов:");
                    msg.AppendLine();

                    foreach (var server in serversResult.Payload)
                    {
                        msg.AppendLine(
                            server.Available ?
                            $"✅ {server.Name}" :
                            $"❌ {server.Name}\n (offline {server.UnavailableSinceString})".ToEscapeMarkDown());

                        msg.AppendLine();
                    }
                }

                return await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: msg.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: new ReplyKeyboardRemove());
            }

            async Task<Message> SendClientsList(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var chatId = message.Chat.Id;
                var textBuilder = new StringBuilder();
                IReplyMarkup keyboard = new ReplyKeyboardRemove();

                var clients = await _mediator.Send(new GetClientsQuery
                {
                    UserId = chatId
                });

                if (clients.IsError)
                {
                    textBuilder.AppendLine("❗️При попытке получить список клиентов произошла ошибка❗️");
                    clients.Errors.ForEach(e => textBuilder.AppendLine(e.Message.ToEscapeMarkDown()));
                }
                else if (clients.Payload == null || !clients.Payload.Any())
                {
                    textBuilder.AppendLine("У вас нет ни одного клиента\\.");
                    textBuilder.AppendLine("/add_client - добавить клиент".ToEscapeMarkDown());
                    textBuilder.AppendLine($"Максимальное кол-во клиентов: {max_clients}".ToEscapeMarkDown());
                }
                else
                {
                    textBuilder.AppendLine("Ваши клиенты:");

                    var buttonRows = new List<List<InlineKeyboardButton>>();

                    foreach (var client in clients.Payload)
                    {
                        var clientName = client.Server.Available ?
                        $"✅ {client.FullName}" :
                        $"❌ {client.FullName}\n (offline {client.Server.UnavailableSinceString})";

                        var buttonRow = new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData(clientName, $"client_actions|{client.Id}")
                    };
                        buttonRows.Add(buttonRow);
                    }

                    keyboard = new InlineKeyboardMarkup(buttonRows);
                }

                return await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: textBuilder.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: keyboard);
            }
        }

        public async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery query)
        {
            string[] args = query.Data.Split('|');

            if (args != null && args.Length > 0)
            {
                var command = args[0];

                var action = command switch
                {
                    "client_actions" => SendClientActions(botClient, args[1]),
                    "client_qr" => SendClientQR(botClient, args[1]),
                    "client_config" => SendClientConfig(botClient, args[1], args[2]),
                    "client_stats" => SendClientStats(botClient, args[1]),
                    "client_delete" => SendClientDelete(botClient, args[1])
                };

                await action;

                async Task<Message> SendClientActions(ITelegramBotClient botClient, string clientId)
                {
                    await botClient.SendChatActionAsync(query.Message!.Chat.Id, ChatAction.Typing);

                    var textBuilder = new StringBuilder();
                    IReplyMarkup keyboard = new ReplyKeyboardRemove();

                    var clientResult = await _mediator.Send(new GetClientQuery
                    {
                        ClientId = Guid.Parse(clientId)
                    });

                    if (clientResult.IsError)
                    {
                        textBuilder.AppendLine("❗️Во время загрузки клиента произошла ошибка❗️");
                        clientResult.Errors.ForEach(e => textBuilder.AppendLine(e.Message.ToEscapeMarkDown()));
                    }
                    else
                    {
                        textBuilder.AppendLine($"Выберите действие для клиента *{clientResult.Payload.FullName.ToEscapeMarkDown()}*");
                        var buttonRows = new List<List<InlineKeyboardButton>>()
                        {
                            new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("🖼 Показать QR-код", $"client_qr|{clientId}")
                            },
                            new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("📄 Скачать файл .conf", $"client_config|{clientId}|true")
                            },
                            new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("📊 Статистика", $"client_stats|{clientId}")
                            },
                            new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("🗑 Удалить клиент", $"client_delete|{clientId}")
                            },
                        };

                        keyboard = new InlineKeyboardMarkup(buttonRows);
                    }

                    return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: textBuilder.ToString(),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: keyboard);
                }

                async Task<Message> SendClientQR(ITelegramBotClient botClient, string clientId)
                {
                    await botClient.SendChatActionAsync(query.Message!.Chat.Id, ChatAction.UploadPhoto);

                    var textBuilder = new StringBuilder();
                    IReplyMarkup keyboard = new ReplyKeyboardRemove();

                    var qrCode = await _mediator.Send(new GetClientQrCodeQuery
                    {
                        ClientId = Guid.Parse(clientId)
                    });

                    if (qrCode.IsError)
                    {
                        textBuilder.AppendLine("❗️При попытке получить QR-код призошла ошибка❗️");
                        qrCode.Errors.ForEach(e => textBuilder.AppendLine(e.Message));
                    }
                    else
                    {
                        textBuilder.AppendLine($"*{qrCode.Payload.ClientName.ToEscapeMarkDown()}*");
                        try
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                qrCode.Payload.Image.Save(ms, new PngEncoder());
                                ms.Position = 0;
                                textBuilder.AppendLine("ℹ️ Используйте этот QR-код для создания тоннеля в приложении WireGuard".ToEscapeMarkDown());

                                return await botClient.SendPhotoAsync(
                                    chatId: query.Message!.Chat.Id,
                                    photo: ms,
                                    parseMode: ParseMode.MarkdownV2,
                                    caption: textBuilder.ToString(),
                                    replyMarkup: keyboard);
                            }
                        }
                        catch (Exception ex)
                        {
                            return await botClient.SendTextMessageAsync(
                                chatId: query.Message!.Chat.Id,
                                text: "При попытке получить QR-код произошло исключение: " + ex.Message,
                                replyMarkup: keyboard);
                        }
                    }

                    return await botClient.SendTextMessageAsync(
                        chatId: query.Message!.Chat.Id,
                        text: textBuilder.ToString(),
                        replyMarkup: keyboard);
                }

                async Task<Message> SendClientConfig(ITelegramBotClient botClient, string clientId, string full_name)
                {
                    await botClient.SendChatActionAsync(query.Message!.Chat.Id, ChatAction.UploadDocument);

                    var text = new StringBuilder();
                    IReplyMarkup keyboard = new ReplyKeyboardRemove();
                                        
                    try
                    {
                        bool fullName = bool.Parse(full_name);
                        var clientConf = await _mediator.Send(new GetClientConfFileQuery
                        {
                            ClientId = Guid.Parse(clientId),
                            FullName = fullName
                        });

                        if (clientConf.IsError)
                        {
                            clientConf.Errors.ForEach(e => text.AppendLine(e.Message));
                            throw new Exception(text.ToString());
                        }
                        else
                        {
                            text.AppendLine($"*{clientConf.Payload.ClientName.ToEscapeMarkDown()}*");
                            text.AppendLine("ℹ️ Используйте этот файл для создания тоннеля в приложении WireGuard\\.");

                            if (fullName)
                            {
                                text.AppendLine();
                                text.AppendLine("❗️Если при попытке добавить тоннель вы получаете ошибку *Неправильное имя*, то скорее всего имя файла получилось слишком длинным\\.");
                                text.AppendLine("Можете переименовать файл вручную или попробовать сгенерировать имя покороче по кнопке ниже 👇");

                                var buttonRows = new List<List<InlineKeyboardButton>>()
                            {
                                new List<InlineKeyboardButton>
                                {
                                    InlineKeyboardButton.WithCallbackData("Получить короткое имя", $"client_config|{clientId}|false")
                                },
                            };

                                keyboard = new InlineKeyboardMarkup(buttonRows);
                            }

                            using (Stream stream = new MemoryStream(clientConf.Payload.File.FileContents))
                            {
                                return await botClient.SendDocumentAsync(
                                    chatId: query.Message!.Chat.Id,
                                    document: new InputOnlineFile(content: stream, fileName: clientConf.Payload.File.FileName),
                                    parseMode: ParseMode.MarkdownV2,
                                    caption: text.ToString(),
                                    replyMarkup: keyboard);
                            }
                        }
                    }
                    catch (NotFoundException)
                    {
                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: "Клиент не найден",
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    catch (Exception ex)
                    {
                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: "При попытке получить .conf файл произошло исключение: " + ex.Message,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                }

                async Task<Message> SendClientStats(ITelegramBotClient botClient, string clientId)
                {
                    await botClient.SendChatActionAsync(query.Message!.Chat.Id, ChatAction.Typing);

                    try
                    {
                        var output = new StringBuilder();
                        
                        var clientStats = await _mediator.Send(new GetClientStatsQuery
                        {
                            ClientId = Guid.Parse(clientId)
                        });

                        if (clientStats.IsError)
                        {
                            output.AppendLine("❗️При получении статистики клиента произошла ошибка❗️");
                            clientStats.Errors.ForEach(e => output.AppendLine(e.Message.ToEscapeMarkDown()));
                        }
                        else
                        {
                            output.AppendLine($"📊 *{clientStats.Payload.ClientName.ToEscapeMarkDown()}*");
                            output.AppendLine($"⬇️ Получено: {clientStats.Payload.BytesReceived}".ToEscapeMarkDown());
                            output.AppendLine($"⬆️ Отправлено: {clientStats.Payload.BytesSent}".ToEscapeMarkDown());
                            output.AppendLine($"🕔 Последнее подключение: {clientStats.Payload.LastSeen}".ToEscapeMarkDown());
                            output.AppendLine();
                            output.AppendLine("Показатель *Получено* показывает количество данных, которое вы отправили клиенту, например, ваш запрос на открытие сайта\\.");
                            output.AppendLine();
                            output.AppendLine("Показатель *Отправлено* показывает количество данных, которое клиент отправил вам\\. Например, если вы хотели открыть сайт, то ответом в данном случае будет сайт целиком, поэтому этот показатель, как правило, больше\\.");
                        }

                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: output.ToString(),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    catch (NotFoundException)
                    {
                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: "Клиент не найден",
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    catch (Exception ex)
                    {
                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: "При поиске статистики клиента произошло исключение: " + ex.Message,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                }

                async Task<Message> SendClientDelete(ITelegramBotClient botClient, string clientId)
                {
                    await botClient.SendChatActionAsync(query.Message!.Chat.Id, ChatAction.Typing);

                    try
                    {
                        var textBuilder = new StringBuilder();

                        var client = await _mediator.Send(new DeleteClientCommand
                        {
                            ClientId = Guid.Parse(clientId)
                        });

                        if (client.IsError)
                        {
                            textBuilder.AppendLine("❗️При попытке удалить клиент возникла ошибка❗️");
                            client.Errors.ForEach(e => textBuilder.AppendLine(e.Message.ToEscapeMarkDown()));
                        }
                        else
                        {
                            textBuilder.AppendLine($"Клиент *{client.Payload.FullName.ToEscapeMarkDown()}* успешно удален\\.");
                            textBuilder.AppendLine("ℹ️ Не забудьте так же удалить тунель для этого клиента в приложении WireGuard\\.");
                        }

                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: textBuilder.ToString(),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    catch (Exception ex)
                    {
                        return await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            text: "При попытке удалить клиент произошло исключение: " + ex.Message,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                }
            }
        }

        public Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
