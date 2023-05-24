using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Clients.Commands.CreateClient
{
    public sealed record CreateClientCommand : IRequest<OperationResult<Client>>
    {
        public long UserId { get; set; }

        public int ServerId { get; set; }

        public string ClientName { get; set; }
    }

    public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, OperationResult<Client>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;

        public CreateClientCommandHandler(IApplicationDbContext context, IPiVPNService piVPNService)
        {
            _context = context;
            _piVPNService = piVPNService;
        }

        public async Task<OperationResult<Client>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Client>();

            try
            {
                var server = await _context.Servers
                    .SingleOrDefaultAsync(s => s.Id == request.ServerId);
                
                if (server == null)
                {
                    result.AddError(ErrorCode.NotFound, $"Server with ID:{request.ServerId} not found!");
                    return result;
                }

                var client = new Client
                {
                    Name = request.ClientName,
                    UserId = request.UserId,
                    ServerId = request.ServerId
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync(cancellationToken);
                _piVPNService.AddNewClient(client.Id.ToString(), server);
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
