using PiVPNManager.Application.Common.Interfaces;

namespace PiVPNManager.WebUI.Services
{
    internal sealed class BotService : IHostedService
    {
        private CancellationTokenSource _cts;
        private readonly ILogger<BotService> _logger;
        private readonly IBot _bot;
               
        public BotService(ILogger<BotService> logger, IBot bot)
        {
            _logger = logger;
            _bot = bot;
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _bot.StartReceivingAsync(_cts.Token);            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _cts.Dispose();
            return Task.CompletedTask;
        }
    }
}
