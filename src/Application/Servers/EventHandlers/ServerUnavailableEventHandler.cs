using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Domain.Enums;
using PiVPNManager.Domain.Events;

namespace PiVPNManager.Application.Servers.EventHandlers
{
    public sealed class ServerUnavailableEventHandler : INotificationHandler<ServerUnavailableEvent>
    {
        private readonly ILogger<ServerAvailableEventHandler> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IBot _bot;

        public ServerUnavailableEventHandler(ILogger<ServerAvailableEventHandler> logger, IApplicationDbContext context, IBot bot)
        {
            _logger = logger;
            _context = context;
            _bot = bot;
        }

        public async Task Handle(ServerUnavailableEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogError("Server {Server} is Unvailable!", notification.Server.Name);

            var serverClients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.ServerId ==  notification.Server.Id)
                .ToListAsync();

            if (serverClients.Any())
            {
                await _bot.SendServerStatusAsync(
                    ServerStatus.Unavailable,
                    notification.Server.Name,
                    serverClients);
            }
        }
    }
}
