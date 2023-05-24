using MediatR;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Enums;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Application.Common.Models;
using PiVPNManager.Application.Servers;
using System.Text;

namespace PiVPNManager.Application.Clients.Queries.GetClientQrCode
{
    public sealed record GetClientQrCodeQuery : IRequest<OperationResult<ClientQrCodeDTO>>
    {
        public Guid ClientId { get; set; }
    }

    public sealed class GetClientQrCodeQueryHandler : IRequestHandler<GetClientQrCodeQuery, OperationResult<ClientQrCodeDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPiVPNService _piVPNService;
        private readonly IQrCodeService _qrCodeService;        

        public GetClientQrCodeQueryHandler(IApplicationDbContext context, IPiVPNService piVPNService, IQrCodeService qrCodeService)
        {
            _context = context;
            _piVPNService = piVPNService;
            _qrCodeService = qrCodeService;
        }

        public async Task<OperationResult<ClientQrCodeDTO>> Handle(GetClientQrCodeQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<ClientQrCodeDTO>();

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

                const int BufferSize = 128;
                var fileName = $"{client.Id}.conf";
                var confBuilder = new StringBuilder();

                using (var fs = new FileStream(Path.Combine("Data", Path.GetFileName(fileName)), FileMode.OpenOrCreate))
                {
                    _piVPNService.DownloadClientConfFile(client.Id.ToString(), client.Server, fs);
                                    
                    fs.Position = 0;
                    using (var streamReader = new StreamReader(fs, Encoding.UTF8, true, BufferSize))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            confBuilder.AppendLine(line);
                        }
                    }
                    var qrImage = _qrCodeService.GenerateQrCodeFromString(confBuilder.ToString());
                    result.Payload = new ClientQrCodeDTO
                    {
                        ClientId = client.Id,
                        ClientName = client.FullName,
                        Image = qrImage
                    };
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
