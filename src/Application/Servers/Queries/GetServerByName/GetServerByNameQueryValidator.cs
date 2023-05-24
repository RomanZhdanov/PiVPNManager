using FluentValidation;

namespace PiVPNManager.Application.Servers.Queries.GetServerByName
{
    public sealed class GetServerByNameQueryValidator : AbstractValidator<GetServerByNameQuery>
    {
        public GetServerByNameQueryValidator()
        {
            RuleFor(v => v.ServerName)
                .NotEmpty().WithMessage("Server Name is required.");
        }
    }
}
