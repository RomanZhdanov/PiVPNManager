using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Application.Servers;

namespace PiVPNManager.Application.Clients.Queries.GetClientStats
{
    public sealed record GetClientStatsQuery : IRequest<OperationResult<ClientStatsDTO>>
    {
        public Guid ClientId { get; set; }
    }

    public sealed class GetClientStatsQueryHandler : IRequestHandler<GetClientStatsQuery, OperationResult<ClientStatsDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;

        public GetClientStatsQueryHandler(IApplicationDbContext context, IPiVPNService piVPNService)
        {
            _context = context;
            _piVPNService = piVPNService;
        }

        public async Task<OperationResult<ClientStatsDTO>> Handle(GetClientStatsQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<ClientStatsDTO>();

            try
            {
                var client = await _context.Clients
                    .AsNoTracking()
                    .Include(c => c.Server)
                    .FirstOrDefaultAsync(c => c.Id == request.ClientId);

                if (client is null)
                {
                    result.AddError(ErrorCode.NotFound,
                        string.Format(ServersErrorMessages.ServerNotFound, request.ClientId));
                    return result;
                }

                var statsString = _piVPNService.GetClientsStats(client.Server);

                using (StringReader reader = new StringReader(statsString))
                {
                    string line;
                    int linecnt = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        linecnt++;
                        if (linecnt > 2)
                        {
                            var st = line.Split("  ",  StringSplitOptions.RemoveEmptyEntries);
                            if (st.Length == 6 && st[0] == client.Id.ToString())
                            {
                                result.Payload = new ClientStatsDTO
                                {
                                    ClientId = st[0],
                                    ClientName = client.FullName,
                                    RemoteIP = st[1],
                                    VirtualIP = st[2],
                                    BytesReceived = st[3],
                                    BytesSent = st[4],
                                    LastSeen = st[5]
                                };

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddUnknownError(ex.Message);
            }

            return result;
        }
    }
}
