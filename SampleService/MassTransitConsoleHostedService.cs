using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleService
{
    public class MassTransitConsoleHostedService :
        IHostedService
    {
        readonly IBusControl _bus;
        readonly ISecondBus _secondBus;
        readonly ILogger _logger;

        public MassTransitConsoleHostedService(IBusControl bus, ISecondBus secondBus, ILoggerFactory loggerFactory)
        {
            _bus = bus;
            _secondBus = secondBus;
            _logger = loggerFactory.CreateLogger<MassTransitConsoleHostedService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bus");
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bus");
            return _bus.StopAsync(cancellationToken);
        }
    }
}