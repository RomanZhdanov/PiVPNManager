using PiVPNManager.Application.Common.Enums;

namespace PiVPNManager.Application.Common.Models
{
    public sealed class Error
    {
        public ErrorCode Code { get; set; }
        
        public string Message { get; set; }
    }
}
