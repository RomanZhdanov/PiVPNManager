using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Servers.Commands.DeleteServer;
using PiVPNManager.Application.Servers.Queries.GetServer;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Servers
{
    public class DeleteModel : PageModel
    {
        private readonly ISender _mediator;

        public DeleteModel(ISender mediator)
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

            var result = await _mediator.Send(new GetServerQuery()
            {
                ServerId = id.Value
            });

            if (result.IsError)
            {
                return NotFound();
            }
            else 
            {
                Server = result.Payload;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var result = await _mediator.Send(new DeleteServerCommand()
            { 
                ServerId = id.Value
            });

            if (result.IsError)
            {
                return BadRequest();
            }

            return RedirectToPage("./Index");
        }
    }
}
