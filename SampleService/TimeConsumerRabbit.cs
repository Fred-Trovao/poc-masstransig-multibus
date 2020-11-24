using System;
using System.Threading.Tasks;
using MassTransit;
using SampleService.Contracts;

namespace SampleService
{
    public class TimeConsumerRabbit :
        IConsumer<IsItTimeRabbit>
    {
        public Task Consume(ConsumeContext<IsItTimeRabbit> context)
        {
            var now = DateTimeOffset.Now;
            return Task.CompletedTask;
        }
    }
}