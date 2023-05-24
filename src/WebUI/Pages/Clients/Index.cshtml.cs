using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiVPNManager.Application.Clients.Queries.GetClients;
using PiVPNManager.Domain.Entities;

namespace PiVPNManager.WebUI.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ISender _mediator;

        public IndexModel(ISender mediator)
        {
            _mediator = mediator;
        }
                
        public IList<Client> Client { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new GetClientsQuery());

            if (!result.IsError)
            {
                Client = result.Payload;
            }
        }
    }
}
