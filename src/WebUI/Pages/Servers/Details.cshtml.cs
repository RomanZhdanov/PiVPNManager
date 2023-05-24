using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Servers.Queries.GetServer;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Servers
{
    public class DetailsModel : PageModel
    {
        private readonly ISender _mediator;

        public DetailsModel(ISender mediator)
        {
            _mediator = mediator;
        }

      public Server Server { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _mediator.Send(new GetServerQuery
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
    }
}
