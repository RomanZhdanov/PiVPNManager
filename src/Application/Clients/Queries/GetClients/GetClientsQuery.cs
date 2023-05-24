using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Clients.Queries.GetClients
{
    public sealed record GetClientsQuery : IRequest<OperationResult<IList<Client>>>
    {
        public long UserId { get; set; } = default;
    }

    public sealed class GetClientQueryHandler : IRequestHandler<GetClientsQuery, OperationResult<IList<Client>>>
    {
        private readonly IApplicationDbContext _context;

        public GetClientQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<IList<Client>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<IList<Client>>();
            
            try
            {
                var query = _context.Clients
                    .AsNoTracking();

                if (request.UserId != default)
                {
                    query = query.Where(c => c.UserId == request.UserId);
                }

                var clients = await query
                    .Include(c => c.Server)
                    .ToListAsync();                
              
                result.Payload = clients;
            }
            catch (Exception e)
            {
                result.AddUnknownError(e.Message);
            }

            return result;
        }
    }
}
