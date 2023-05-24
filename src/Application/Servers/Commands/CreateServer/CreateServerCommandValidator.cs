using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PiVPNManager.Application.Common.Interfaces;

namespace PiVPNManager.Application.Servers.Commands.CreateServer
{
    public sealed class CreateServerCommandValidator : AbstractValidator<CreateServerCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateServerCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
                .MustAsync(BeUniqueName).WithMessage("The specified name already exists.");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return await _context.Servers
                .AllAsync(l => l.Name != name, cancellationToken);
        }
    }
}
