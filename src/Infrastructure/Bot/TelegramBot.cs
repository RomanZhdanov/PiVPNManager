using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Domain.Entities;
using PiVPNManager.Domain.Enums;
using PiVPNManager.Infrastructure.Bot.Handlers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace PiVPNManager.Infrastructure.Bot
{
    public sealed class TelegramBot : IBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly ITelegramBotClient _bot;
        private readonly IBotHandlers _handlers;

        public TelegramBot(ILogger<TelegramBot> logger, IConfiguration configuration, IBotHandlers handlers)
        {
            _logger = logger;
            _handlers = handlers;

            var apiKey = configuration["Telegram:ApiKey"];

            if (apiKey == null)
            {
                throw new Exception("ApiKey for telegram bot is not found!");
            }

            _bot = new TelegramBotClient(apiKey);
        }

        public async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken);

            _bot.StartReceiving(
                updateHandler: _handlers.HandleUpdateAsync,
                pollingErrorHandler: _handlers.HandleErrorAsync,
                receiverOptions: new ReceiverOptions()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cancellationToken: cancellationToken);

            _logger.LogInformation($"Start listening for @{me.Username}");
        }

        public async Task SendTextMessageAsync(long chatId, string text)
        {
            await _bot.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.MarkdownV2);
        }

        public async Task SendServerStatusAsync(ServerStatus status, string serverName, IList<Client> clients)
        {
            foreach (var user in clients.GroupBy(c => c.UserId))
            {
                var msg = new StringBuilder();

                if (status == ServerStatus.Unavailable)
                {
                    msg.AppendLine($"❌ *{serverName.ToEscapeMarkDown()}* \\- сервер недоступен❗️");
                    msg.AppendLine();
                    msg.AppendLine("ℹ️ Список ваших клиентов, которые в данный момент не работают:");
                }

                if (status == ServerStatus.Available)
                {
                    msg.AppendLine($"✅ *{serverName.ToEscapeMarkDown()}* \\- сервер возобновил свою работу\\!");
                    msg.AppendLine();
                    msg.AppendLine("ℹ️ Следующими клиентами снова можно пользоваться:");
                }

                foreach (var client in user)
                {
                    msg.AppendLine($"• *{client.Name?.ToEscapeMarkDown()}*");
                }

                try
                {
                    await _bot.SendTextMessageAsync(
                        chatId: user.Key,
                        text: msg.ToString(),
                        parseMode: ParseMode.MarkdownV2);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
