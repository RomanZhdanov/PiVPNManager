using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Domain.Enums;
using PiVPNManager.Domain.Events;

namespace PiVPNManager.Application.Servers.EventHandlers
{
    public sealed class ServerAvailableEventHandler : INotificationHandler<ServerAvailableEvent>
    {
        private readonly ILogger<ServerAvailableEventHandler> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IBot _bot;

        public ServerAvailableEventHandler(ILogger<ServerAvailableEventHandler> logger, IApplicationDbContext context, IBot bot)
        {
            _logger = logger;
            _context = context;
            _bot = bot;
        }

        public async Task Handle(ServerAvailableEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Server {Server} is Available again!", notification.Server.Name);

            var serverClients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.ServerId ==  notification.Server.Id)
                .ToListAsync();

            if (serverClients.Any())
            {
                await _bot.SendServerStatusAsync(
                    ServerStatus.Available,
                    notification.Server.Name,
                    serverClients);
            }
        }
    }
}
