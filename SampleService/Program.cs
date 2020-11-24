using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Amazon.Runtime;
using MassTransit;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.AspNetCoreIntegration;
using MassTransit.MultiBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleService.Contracts;

namespace SampleService
{
    class Program
    {
        public static AppConfig AppConfig { get; set; }

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<TimeConsumerRabbit>();
                        
                        cfg.UsingRabbitMq((context, config) =>
                        {
                            config.Host(new Uri("amqp://guest:guest@rabbitmq:5672"));

                            config.ReceiveEndpoint("testando-tiago-rosendo",
                                e => { e.ConfigureConsumer<TimeConsumerRabbit>(context); });
                        });
                    });

                    services.AddMassTransit<ISecondBus>(cfg =>
                    {
                        cfg.AddConsumer<TimeConsumerRabbit>();

                        cfg.UsingAmazonSqs((context, config) =>
                        {
                            config.Host("us-east-1", h =>
                            {
                                h.Credentials(new SessionAWSCredentials("",
                                    "",
                                    ""));
                            });
                        
                            config.ConfigureEndpoints(context);
                        });
                    });

                    services.AddMassTransitHostedService();
                    services.AddHostedService<CheckTheTimeService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            if (isService)
            {
                await builder.UseWindowsService().Build().RunAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
    }
}