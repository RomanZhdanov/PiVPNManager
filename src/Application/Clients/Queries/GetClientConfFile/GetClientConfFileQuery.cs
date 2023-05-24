using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Exceptions;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Clients.Queries.GetClientConfFile
{
    public sealed record GetClientConfFileQuery : IRequest<OperationResult<ClientConfFileDTO>>
    {
        public Guid ClientId { get; set; }

        public bool FullName { get; set; }
    }

    public sealed class GetClientConfFileQueryHandler : IRequestHandler<GetClientConfFileQuery, OperationResult<ClientConfFileDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;

        public GetClientConfFileQueryHandler(IApplicationDbContext context, IPiVPNService piVPNService)
        {
            _context = context;
            _piVPNService = piVPNService;
        }

        public async Task<OperationResult<ClientConfFileDTO>> Handle(GetClientConfFileQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<ClientConfFileDTO>();

            try
            {
                var client = await _context.Clients
                    .AsNoTracking()
                    .Include(c => c.Server)
                    .FirstOrDefaultAsync(c => c.Id == request.ClientId);

                if (client is null)
                {
                    // this is actually bad for performance
                    // need to come up with better solution
                    // but right now it feels quite convenient
                    throw new NotFoundException(nameof(Client), request.ClientId);
                }

                var name = request.FullName ? client.FullName : client.Name;
                var fileName = $"{name}.conf";                

                using (var fs = new FileStream(Path.Combine("Data", Path.GetFileName(fileName)), FileMode.OpenOrCreate))
                {
                    _piVPNService.DownloadClientConfFile(client.Id.ToString(), client.Server, fs);
                    
                    var conf = new FileResponse
                    {
                        FileName = fileName,
                        FileContents = new byte[fs.Length],
                        ContentType = "application/octet-stream"
                    };

                    fs.Position = 0;
                    fs.Read(conf.FileContents, 0, conf.FileContents.Length);

                    result.Payload = new ClientConfFileDTO
                    {
                        ClientId = client.Id,
                        ClientName = client.FullName,
                        File = conf
                    };
                }
            }
            catch (NotFoundException ex)
            {
                result.AddError(ErrorCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                result.AddUnknownError(ex.Message);
            }

            return result;
        }
    }
}
