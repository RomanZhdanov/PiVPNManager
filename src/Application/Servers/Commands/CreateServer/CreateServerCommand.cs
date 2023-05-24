using MediatR;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Commands.CreateServer
{
    public sealed record CreateServerCommand : IRequest<OperationResult<Server>>
    {
        public string? Name { get; set; }

        public string? Host { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }

    public sealed class CreateServerCommandHandler : IRequestHandler<CreateServerCommand, OperationResult<Server>>
    {
        private readonly IApplicationDbContext _context;

        public CreateServerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Server>> Handle(CreateServerCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Server>();

            try
            {
                var server = new Server
                {
                    Name = request.Name,
                    Host = request.Host,
                    Username = request.Username,
                    Password = request.Password,
                    Available = true,
                    Dead = false
                };

                _context.Servers.Add(server);
                await _context.SaveChangesAsync(cancellationToken);

                result.Payload = server;
            }
            catch (Exception ex)
            {
                result.AddUnknownError(ex.Message);
            }

            return result;
        }
    }
}
