using LibLiveVpn_ServerWorker.Infrastructure.Consumers;
using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands;
using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Events;
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

            services.AddMassTransit(c =>
            {
                c.AddConsumer<CreateInterfaceCommandConsumer>();
                c.AddConsumer<UpdateInterfaceCommandConsumer>();
                c.AddConsumer<DeleteInterfaceCommandConsumer>();

                c.UsingRabbitMq((context, settings) =>
                {
                    settings.Host(workerConfiguration.BrokerHost, workerConfiguration.BrokerPort, "/", s =>
                    {
                        s.ConnectionName($"worker-node-{workerConfiguration.Id}-connection");
                        s.Username(workerConfiguration.BrokerUsername);
                        s.Password(workerConfiguration.BrokerPassword);
                    });

                    settings.Message<CreateInterfaceCommand>(x => x.SetEntityName(nameof(CreateInterfaceCommand)));
                    settings.Message<UpdateInterfaceCommand>(x => x.SetEntityName(nameof(UpdateInterfaceCommand)));
                    settings.Message<DeleteInterfaceCommand>(x => x.SetEntityName(nameof(DeleteInterfaceCommand)));
                    settings.Message<WorkerStartedEvent>(x => x.SetEntityName(nameof(WorkerStartedEvent)));

                    settings.ReceiveEndpoint($"worker-node-{workerConfiguration.Id}", endpoint =>
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
