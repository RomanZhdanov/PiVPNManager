using SixLabors.ImageSharp;

namespace PiVPNManager.Application.Clients.Queries.GetClientQrCode
{
    public sealed class ClientQrCodeDTO
    {
        public Guid ClientId { get; set; }

        public string? ClientName { get; set; }

        public Image Image { get; set; }
    }
}
