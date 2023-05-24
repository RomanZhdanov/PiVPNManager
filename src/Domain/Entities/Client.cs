namespace PiVPNManager.Domain.Entities
{
    public sealed class Client
    {
        public Guid Id { get; set; }

        public int ServerId { get; set; }

        public long UserId { get; set; }

        public string? Name { get; set; }

        public Server Server { get; set; }

        public string FullName
        {
            get
            {
                return Server == null ? Name! : $"{Name} ({Server.Name})";
            }
        }
    }
}
