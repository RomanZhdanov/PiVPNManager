using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Servers.Commands.UpdateServer;
using PiVPNManager.Application.Servers.Queries.GetServer;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Servers
{
    public class EditModel : PageModel
    {
        private readonly ISender _mediator;

        public EditModel(ISender mediator)
        {
            _mediator = mediator;
        }        

        [BindProperty]
        public Server Server { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result  =  await _mediator.Send(new GetServerQuery
            {
                ServerId = id.Value
            });

            if (result.IsError)
            {
                return NotFound();
            }
            Server = result.Payload;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _mediator.Send(new UpdateServerCommand
            {
                ServerId = Server.Id,
                Name = Server.Name,
                Host = Server.Host,
                Username = Server.Username,
                Password = Server.Password,
                Dead = Server.Dead,
            });

            return RedirectToPage("./Index");
        }
    }
}
