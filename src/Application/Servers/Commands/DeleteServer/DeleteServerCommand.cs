using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Commands.DeleteServer
{
    public sealed record DeleteServerCommand : IRequest<OperationResult<Server>>
    {
        public int ServerId { get; set; }
    }

    public sealed class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand, OperationResult<Server>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteServerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Server>> Handle(DeleteServerCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Server>();

            try
            {
                var server = await _context.Servers
                    .FirstOrDefaultAsync(s => 
                        s.Id == request.ServerId);

                if (server is null)
                {
                    result.AddError(ErrorCode.NotFound,
                        string.Format(ServersErrorMessages.ServerNotFound, request.ServerId));

                    return result;
                }                

                _context.Servers.Remove(server);
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
