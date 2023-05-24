using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Clients.Queries.GetClient
{
    public sealed record GetClientQuery : IRequest<OperationResult<Client>>
    {
        public Guid ClientId { get; set; }
    }

    public sealed class GetClientQueryHandler : IRequestHandler<GetClientQuery, OperationResult<Client>>
    {
        private readonly IApplicationDbContext _context;

        public GetClientQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Client>> Handle(GetClientQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Client>();

            var client = await _context.Clients
                .AsNoTracking()
                .Include(c => c.Server)
                .SingleOrDefaultAsync(c => c.Id == request.ClientId);

            if (client is null)
            {
                result.AddError(ErrorCode.NotFound, $"Client with ID {request.ClientId} is not found.");
                return result;
            }

            result.Payload = client;

            return result;
        }
    }
}
