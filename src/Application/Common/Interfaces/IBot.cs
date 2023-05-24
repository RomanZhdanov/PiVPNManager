using PiVPNManager.Domain.Entities;
using PiVPNManager.Domain.Enums;

namespace PiVPNManager.Application.Common.Interfaces
{
    public interface IBot
    {
        Task StartReceivingAsync(CancellationToken cancellationToken);

        Task SendServerStatusAsync(ServerStatus status, string serverName, IList<Client> clients);

        Task SendTextMessageAsync(long chatId, string text);
    }
}
