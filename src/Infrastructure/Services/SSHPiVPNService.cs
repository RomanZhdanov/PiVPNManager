using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Domain.Entities;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace PiVPNManager.Infrastructure.Services
{
    public sealed class SSHPiVPNService : IPiVPNService
    {
        public string AddNewClient(string clientName, Server server)
        {
            using (var client = CreateSshClient(server))
            {
                client.Connect();

                var output = client.RunCommand($"pivpn -a -n {clientName}").Result;

                client.Disconnect();

                return output;
            }
        }

        public bool CanConnectToServer(Server server)
        {
            using (var client = CreateSshClient(server))
            {   
                try
                {
                    client.Connect();
                    client.Disconnect();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public string DeleteClient(string clientName, Server server)
        {
            using (var client = CreateSshClient(server))
            {
                client.Connect();

                var output = client.RunCommand($"pivpn -r -y {clientName}").Result;

                client.Disconnect();

                return output;
            }
        }

        public void DownloadClientConfFile(string clientName, Server server, Stream output)
        {
            var filePath = "/home/vpn/configs/" + clientName + ".conf";

            using (var sftpClient = new SftpClient(server.Host, server.Username, server.Password))
            {
                sftpClient.Connect();
                    
                sftpClient.DownloadFile(filePath, output);

                sftpClient.Disconnect();
            }
        }

        public string GetClientsStats(Server server)
        {
            using (var client = CreateSshClient(server))
            {
                client.Connect();

                var output = client.RunCommand($"pivpn -c").Result;

                client.Disconnect();

                return output;
            }
        }

        private SshClient CreateSshClient(Server server)
        {
            var client = new SshClient(server.Host, server.Username, server.Password);
            
            //Accept Host key
            client.HostKeyReceived += delegate (object? sender, HostKeyEventArgs e)
            {
                e.CanTrust = true;
            };

            return client;
        }
    }
}
