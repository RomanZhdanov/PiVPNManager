using Telegram.Bot;
using Telegram.Bot.Types;

namespace PiVPNManager.Infrastructure.Bot.Handlers
{
    public interface IUpdateHandlers
    {
        Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, Dictionary<long, UserStates> usersStates, Dictionary<long, UserClient> userClients);

        Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery query);

        Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update);
    }
}
