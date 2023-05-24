using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;

namespace PiVPNManager.Application.Servers.Commands.ServersHealthCheck
{
    public sealed record ServersHealthCheckCommand : IRequest<OperationResult<object>>;

    public sealed class ServersHealthCheckCommandHandler : IRequestHandler<ServersHealthCheckCommand, OperationResult<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;
        private readonly IDateTime _dateTime;

        public ServersHealthCheckCommandHandler(IApplicationDbContext context, IPiVPNService piVPNService, IDateTime dateTime)
        {
            _context = context;
            _piVPNService = piVPNService;
            _dateTime = dateTime;
        }

        public async Task<OperationResult<object>> Handle(ServersHealthCheckCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<object>();
            var healthCheckDate = _dateTime.Now.UtcDateTime;

            var servers = await _context.Servers.ToListAsync();

            foreach (var server in servers)
            {
                try
                {
                    server.Available = _piVPNService.CanConnectToServer(server);
                    server.LastHealthCheck = healthCheckDate;

                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    result.AddUnknownError(ex.Message);
                }
            }

            result.Payload = null;

            return result;
        }
    }
}
