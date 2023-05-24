using PiVPNManager.Domain.Common;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Domain.Events
{
    public sealed class ServerAvailableEvent : BaseEvent
    {
        public ServerAvailableEvent(Server server)
        {
            Server = server;
        }

        public Server Server { get; set; }
    }
}
