using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Commands.UpdateServer
{
    public sealed record UpdateServerCommand : IRequest<OperationResult<Server>>
    {
        public int ServerId { get; set; }

        public string Name { get; set; }

        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool Dead { get; set; }
    }

    public sealed class UpdateServerCommandHandler : IRequestHandler<UpdateServerCommand, OperationResult<Server>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateServerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Server>> Handle(UpdateServerCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Server>();

            try
            {
                var server = await _context.Servers
                    .FirstOrDefaultAsync(p => p.Id == request.ServerId);

                if (server is null)
                {
                    result.AddError(ErrorCode.NotFound,
                        string.Format(ServersErrorMessages.ServerNotFound, request.ServerId));
                    return result;
                }                

                server.Name = request.Name;
                server.Host = request.Host;
                server.Username = request.Username;
                server.Password = request.Password;
                server.Dead = request.Dead;

                await _context.SaveChangesAsync(cancellationToken);

                result.Payload = server;
            }            
            catch (Exception e)
            {
                result.AddUnknownError(e.Message);
            }

            return result;
        }
    }
}
