using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Servers.Commands.CreateServer;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Servers
{
    public class CreateModel : PageModel
    {
        private readonly ISender _mediator;

        public CreateModel(ISender mediator)
        {
            _mediator = mediator;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Server Server { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || Server == null)
            {
                return Page();
            }

            await _mediator.Send(new CreateServerCommand()
            {
                Name = Server.Name,
                Host = Server.Host,
                Username = Server.Username,
                Password = Server.Password
            });

            return RedirectToPage("./Index");
        }
    }
}
