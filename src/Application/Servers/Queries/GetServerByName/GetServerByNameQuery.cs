using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Queries.GetServerByName
{
    public sealed record GetServerByNameQuery : IRequest<OperationResult<Server>>
    {
        public string ServerName { get; set; }
    }

    public sealed class GetServerByNameQueryHandler : IRequestHandler<GetServerByNameQuery, OperationResult<Server>>
    {
        private readonly IApplicationDbContext _context;

        public GetServerByNameQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Server>> Handle(GetServerByNameQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Server>();

            var server = await _context.Servers
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Name == request.ServerName);

            if (server is null)
            {
                result.AddError(ErrorCode.NotFound,
                    string.Format(ServersErrorMessages.ServerNotFound, request.ServerName));
                return result;
            }

            result.Payload = server;

            return result;
        }
    }
}
