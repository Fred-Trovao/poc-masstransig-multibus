using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using SampleService.Contracts;

namespace SampleService
{
    public class CheckTheTimeService :
        IHostedService
    {
        public IPublishEndpoint PublishEndpoint { get; }
        public ISecondBus SecondBus { get; }
        Timer _timer;

        public CheckTheTimeService(IPublishEndpoint publishEndpoint, ISecondBus secondBus)
        {
            PublishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            SecondBus = secondBus ?? throw new ArgumentNullException(nameof(secondBus));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckTheTime, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        async void CheckTheTime(object state)
        {
            var now = DateTimeOffset.Now;
            if (now.Millisecond % 2 == 0)
            {
                Console.WriteLine("Enviando pelo segundos bus");
                await SecondBus.Publish<IsItTimeRabbit>(new {Texto = "Enviando pelo segundos bus"});
                return;
            }

            Console.WriteLine("Enviando pelo primeiro bus");
            await PublishEndpoint.Publish<IsItTimeRabbit>(new {Texto = "Enviando pelo primeiro bus"});
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();

            return Task.CompletedTask;
        }
    }
}