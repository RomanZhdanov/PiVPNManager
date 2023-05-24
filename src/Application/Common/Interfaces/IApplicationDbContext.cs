using Microsoft.EntityFrameworkCore;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Server> Servers { get; }

        DbSet<Client> Clients { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
