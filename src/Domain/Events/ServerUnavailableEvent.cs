using PiVPNManager.Domain.Common;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Domain.Events
{
    public sealed class ServerUnavailableEvent : BaseEvent
    {
        public ServerUnavailableEvent(Server server)
        {
            Server = server;
        }

        public Server Server { get; }
    }
}
