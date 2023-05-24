using PiVPNManager.Domain.Common;
using PiVPNManager.Domain.Events;
using PiVPNManager.Domain.Extensions;

namespace PiVPNManager.Domain.Entities
{
    public sealed class Server : BaseAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool Dead { get; set; }

        public DateTime? LastHealthCheck { get; set; }

        public DateTime? UnavailableSince { get; private set; }

        private bool _available;

        public bool Available
        {
            get => _available;
            set
            {
                if (value == false && _available == true)
                {
                    UnavailableSince = DateTime.UtcNow;
                    AddDomainEvent(new ServerUnavailableEvent(this));
                }

                if (value == true && _available == false)
                {
                    UnavailableSince = null;
                    AddDomainEvent(new ServerAvailableEvent(this));
                }

                _available = value;
            }
        }

        public string? UnavailableSinceString
        {
            get
            {
                if (UnavailableSince == null) return null;

                return DateTime.UtcNow.Subtract(UnavailableSince.Value)
                    .ToReadableString();
            }
        }
    }
}
