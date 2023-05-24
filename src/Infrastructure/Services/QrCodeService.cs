using PiVPNManager.Application.Common.Interfaces;
using QRCoder;
using SixLabors.ImageSharp;

namespace PiVPNManager.Infrastructure.Services
{
    public sealed class QrCodeService : IQrCodeService
    {
        public Image GenerateQrCodeFromString(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
