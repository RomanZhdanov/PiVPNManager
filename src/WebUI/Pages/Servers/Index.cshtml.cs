using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Servers.Queries.GetServers;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Servers
{
    public class IndexModel : PageModel
    {
        private readonly ISender _mediator;

        public IndexModel(ISender mediator)
        {
            _mediator = mediator;
        }

        public IList<Server> Server { get;set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _mediator.Send(new GetServersQuery());

            if (result.IsError)
            {
                return BadRequest();
            }

            Server = result.Payload;
            return Page();
        }
    }
}
