using PiVPNManager.Application.Common.Interfaces;

namespace PiVPNManager.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
