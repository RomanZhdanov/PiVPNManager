using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Common.Interfaces
{
    public interface IPiVPNService
    {
        bool CanConnectToServer(Server server);

        void DownloadClientConfFile(string clientName, Server server, Stream output);

        string AddNewClient(string clientName, Server server);

        string DeleteClient(string clientName, Server server);

        string GetClientsStats(Server server);
    }
}
