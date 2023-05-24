using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Queries.GetServers
{
    public sealed record GetServersQuery : IRequest<OperationResult<IList<Server>>>
    {
        public bool NotDead { get; set; }

        public bool AvailableOnly { get; set; }     
    }

    public sealed class GetServersQueryHandler : IRequestHandler<GetServersQuery, OperationResult<IList<Server>>>
    {
        private readonly IApplicationDbContext _context;
        
        public GetServersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<IList<Server>>> Handle(GetServersQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<IList<Server>>();

            try
            {
                var query = _context.Servers
                    .AsNoTracking();

                if (request.NotDead)
                {
                    query = query.Where(s => !s.Dead);
                }

                if (request.AvailableOnly)
                {
                    query = query.Where(s => s.Available);
                }

                var servers = await query.ToListAsync();
                result.Payload = servers;
            }
            catch (Exception e)
            {
                result.AddUnknownError(e.Message);
            }

            return result;
        }
    }
}
