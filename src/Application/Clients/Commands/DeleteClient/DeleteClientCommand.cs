using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Clients.Commands.DeleteClient
{
    public sealed record DeleteClientCommand : IRequest<OperationResult<Client>>
    {
        public Guid ClientId { get; set; }
    }

    public sealed class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, OperationResult<Client>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;

        public DeleteClientCommandHandler(IApplicationDbContext context, IPiVPNService piVPNService)
        {
            _context = context;
            _piVPNService = piVPNService;
        }

        public async Task<OperationResult<Client>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Client>();

            try
            {
                var client = await _context.Clients
                    .Include(c => c.Server)
                    .SingleOrDefaultAsync(c => c.Id == request.ClientId);

                if (client == null)
                {
                    result.AddError(ErrorCode.NotFound, $"Client with ID {request.ClientId} is not found!");
                    return result;
                }

                _piVPNService.DeleteClient(client.Id.ToString(), client.Server);
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync(cancellationToken);
                result.Payload = client;
            }
            catch (Exception ex)
            {
                result.AddUnknownError(ex.Message);
            }

            return result;
        }
    }
}
