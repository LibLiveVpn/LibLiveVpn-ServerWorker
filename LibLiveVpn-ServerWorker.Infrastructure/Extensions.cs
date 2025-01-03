using LibLiveVpn_Contracts.Commands;
using LibLiveVpn_Contracts.Events;
using LibLiveVpn_ServerWorker.Application.Interfaces;
using LibLiveVpn_ServerWorker.Infrastructure.Consumers;
using LibLiveVpn_ServerWorker.Infrastructure.Models;
using LibLiveVpn_ServerWorker.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibLiveVpn_ServerWorker.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<FileAccessorService>();
            services.AddSingleton<IVpnServerManager, WireguardServerManager>();

            services.AddMasstransitServices(configuration);

            return services;
        }

        private static IServiceCollection AddMasstransitServices(this IServiceCollection services, IConfiguration configuration)
        {
            var workerConfigurationSection = configuration.GetSection(WorkerConfiguration.Section);

            var workerConfiguration = new WorkerConfiguration();
            workerConfiguration.Id = workerConfigurationSection[nameof(WorkerConfiguration.Id)] ?? throw new ArgumentNullException($"Required field {nameof(WorkerConfiguration.Id)} not exists");
            workerConfiguration.BrokerHost = workerConfigurationSection[nameof(WorkerConfiguration.BrokerHost)] ?? throw new ArgumentNullException($"Required field {nameof(WorkerConfiguration.BrokerHost)} not exists");

            if (ushort.TryParse(workerConfigurationSection[nameof(WorkerConfiguration.BrokerPort)], out ushort brokerPort))
                workerConfiguration.BrokerPort = brokerPort;
            else
                workerConfiguration.BrokerPort = 5672;

            workerConfiguration.BrokerUsername = workerConfigurationSection[nameof(WorkerConfiguration.BrokerUsername)] ?? throw new ArgumentNullException($"Required field {nameof(WorkerConfiguration.BrokerUsername)} not exists");
            workerConfiguration.BrokerPassword = workerConfigurationSection[nameof(WorkerConfiguration.BrokerPassword)] ?? throw new ArgumentNullException($"Required field {nameof(WorkerConfiguration.BrokerPassword)} not exists");

            services.AddSingleton(workerConfiguration);

            services.AddMassTransit(conf =>
            {
                conf.AddConsumer<CreateInterfaceCommandConsumer>();
                conf.AddConsumer<UpdateInterfaceCommandConsumer>();
                conf.AddConsumer<DeleteInterfaceCommandConsumer>();

                conf.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(workerConfiguration.BrokerHost, workerConfiguration.BrokerPort, "/", s =>
                    {
                        s.ConnectionName($"worker-node-{workerConfiguration.Id}-connection");
                        s.Username(workerConfiguration.BrokerUsername);
                        s.Password(workerConfiguration.BrokerPassword);
                    });

                    cfg.Message<CreateInterfaceCommand>(x => x.SetEntityName(nameof(CreateInterfaceCommand)));
                    cfg.Message<UpdateInterfaceCommand>(x => x.SetEntityName(nameof(UpdateInterfaceCommand)));
                    cfg.Message<DeleteInterfaceCommand>(x => x.SetEntityName(nameof(DeleteInterfaceCommand)));

                    cfg.Message<CommandExecudedEvent>(x => x.SetEntityName(nameof(CommandExecudedEvent)));
                    cfg.Message<WorkerStartedEvent>(x => x.SetEntityName(nameof(WorkerStartedEvent)));

                    cfg.ReceiveEndpoint($"worker-node-{workerConfiguration.Id}", endpoint =>
                    {
                        endpoint.ConfigureConsumer<CreateInterfaceCommandConsumer>(context);
                        endpoint.ConfigureConsumer<UpdateInterfaceCommandConsumer>(context);
                        endpoint.ConfigureConsumer<DeleteInterfaceCommandConsumer>(context);
                    });
                });
            });

            return services;
        }

        public static async Task NotifyWorkerStart(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("NorifyWorkerStart");

                var workerConfiguration = scope.ServiceProvider.GetRequiredService<WorkerConfiguration>();
                var brokerPublisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                await brokerPublisher.Publish(new WorkerStartedEvent
                {
                    WorkerId = workerConfiguration.Id
                });

                logger.LogInformation("Worker ready to work");
            }
        }
    }
}
