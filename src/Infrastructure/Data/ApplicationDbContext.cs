using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Domain.Entities;
using PiVPNManager.Infrastructure.Common;

namespace PiVPNManager.Infrastructure.Data
{
    public sealed class ApplicationDbContext : IdentityDbContext, IApplicationDbContext
    {
        private readonly IMediator _mediator;
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Server> Servers => Set<Server>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEvents(this);

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}