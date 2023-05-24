using FluentValidation;

namespace PiVPNManager.Application.Clients.Commands.CreateClient
{
    public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public CreateClientCommandValidator()
        {
            RuleFor(v => v.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(v => v.ClientName)
                .NotEmpty().WithMessage("Client name is required.")
                .MaximumLength(8).WithMessage("Client name must not exceed 8 characters.");

            RuleFor(v => v.ServerId)
                .NotEmpty().WithMessage("Server ID is required.");                
        }        
    }
}
