using MediatR;
using PiVPNManager.Application.Servers.Commands.ServersHealthCheck;

namespace PiVPNManager.WebUI.Services
{
    internal sealed class HealthCheckService : BackgroundService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period;

        public HealthCheckService(IConfiguration configuration, ILogger<HealthCheckService> logger, IServiceProvider serviceProvider)
        {
            _period = TimeSpan.FromMinutes(double.Parse(configuration["HealthCheckPeriodMinutes"]));
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunHealthCheck();

            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await RunHealthCheck();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(
                        $"Failed to execute PeriodicHostedService with exception message {ex.Message}. Good luck next round!");
                }
            }
        }            

        private async Task RunHealthCheck()
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    var hcResult = await mediator.Send(new ServersHealthCheckCommand());

                    if (hcResult.IsError)
                    {
                        _logger.LogError("There were errors during HealthCheck!");
                        hcResult.Errors.ForEach(e => _logger.LogError(e.Message));

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("HealthCheck exception: {exception}", ex.Message);
            }           
        }
    }
}
