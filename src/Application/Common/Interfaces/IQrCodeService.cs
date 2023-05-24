using SixLabors.ImageSharp;

namespace PiVPNManager.Application.Common.Interfaces
{
    public interface IQrCodeService
    {
        Image GenerateQrCodeFromString(string text);
    }
}
