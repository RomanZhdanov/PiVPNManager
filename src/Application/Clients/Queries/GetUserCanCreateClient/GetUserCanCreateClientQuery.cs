using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;

namespace PiVPNManager.Application.Clients.Queries.GetUserCanCreateClient
{
    public sealed record GetUserCanCreateClientQuery : IRequest<bool>
    {
        public long UserId { get; set; }
    }

    public sealed class GetUserCanCreateClientQueryHandler : IRequestHandler<GetUserCanCreateClientQuery, bool>
    {
        private const int max_clients = 2;
        private readonly IApplicationDbContext _context;

        public GetUserCanCreateClientQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(GetUserCanCreateClientQuery request, CancellationToken cancellationToken)
        {
            var clientsCnt = await _context.Clients
                .CountAsync(c => c.UserId == request.UserId);

            return clientsCnt < max_clients;
        }
    }
}
