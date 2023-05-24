namespace PiVPNManager.Application.Clients.Queries.GetClientStats
{
    public sealed class ClientStatsDTO
    {
        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string RemoteIP { get; set; }

        public string VirtualIP { get; set; }

        public string BytesReceived { get; set; }

        public string BytesSent { get; set; }

        public string LastSeen { get; set; }
    }
}
