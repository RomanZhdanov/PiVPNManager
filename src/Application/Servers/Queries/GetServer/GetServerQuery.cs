using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Servers.Queries.GetServer
{
    public sealed record GetServerQuery : IRequest<OperationResult<Server>>
    {
        public int ServerId { get; set; }
    }

    public sealed class GetServerQueryHandler : IRequestHandler<GetServerQuery, OperationResult<Server>>
    {
        private readonly IApplicationDbContext _context;

        public GetServerQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Server>> Handle(GetServerQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<Server>();

            var server = await _context.Servers
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id == request.ServerId);

            if (server is null)
            {
                result.AddError(ErrorCode.NotFound,
                    string.Format(ServersErrorMessages.ServerNotFound, request.ServerId));
                return result;
            }

            result.Payload = server;

            return result;
        }
    }
}
