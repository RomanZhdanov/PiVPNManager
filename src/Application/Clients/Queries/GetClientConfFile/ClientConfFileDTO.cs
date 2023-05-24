using PiVPNManager.Application.Common.Models;

namespace PiVPNManager.Application.Clients.Queries.GetClientConfFile
{
    public sealed class ClientConfFileDTO
    {
        public Guid ClientId { get; set; }

        public string? ClientName { get; set; }

        public FileResponse File { get; set; }
    }
}
