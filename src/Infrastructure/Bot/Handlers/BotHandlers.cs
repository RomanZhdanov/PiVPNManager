using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PiVPNManager.Infrastructure.Bot.Handlers
{
    public sealed class BotHandlers : IBotHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<long, UserStates> _usersStates = new Dictionary<long, UserStates>();
        private Dictionary<long, UserClient> _userClients = new Dictionary<long, UserClient>();

        public BotHandlers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }        

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IUpdateHandlers updateHandlers =
                    scope.ServiceProvider.GetRequiredService<IUpdateHandlers>();                
                
                var handler = update.Type switch
                {
                    UpdateType.Message => updateHandlers.BotOnMessageReceived(botClient, update.Message!, _usersStates, _userClients),
                    UpdateType.EditedMessage => updateHandlers.BotOnMessageReceived(botClient, update.EditedMessage!, _usersStates, _userClients),
                    UpdateType.CallbackQuery => updateHandlers.BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
                    _ => updateHandlers.UnknownUpdateHandlerAsync(botClient, update)
                };

                try
                {
                    await handler;
                }
                catch (Exception exception)
                {
                    await HandleErrorAsync(botClient, exception, cancellationToken);
                }
            }
        }
    }
}
