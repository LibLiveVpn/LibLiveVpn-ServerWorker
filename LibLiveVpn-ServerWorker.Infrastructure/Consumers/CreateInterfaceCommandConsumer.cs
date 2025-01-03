using LibLiveVpn_Contracts.Commands;
using LibLiveVpn_Contracts.Events;
using LibLiveVpn_ServerWorker.Application.Interfaces;
using LibLiveVpn_ServerWorker.Infrastructure.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LibLiveVpn_ServerWorker.Infrastructure.Consumers
{
    public class CreateInterfaceCommandConsumer : IConsumer<CreateInterfaceCommand>
    {
        private readonly ILogger<CreateInterfaceCommandConsumer> _logger;
        private readonly IVpnServerManager _vpnServerManager;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly WorkerConfiguration _workerConfiguration;

        public CreateInterfaceCommandConsumer(ILogger<CreateInterfaceCommandConsumer> logger, IVpnServerManager vpnServerManager, IPublishEndpoint publishEndpoint, WorkerConfiguration workerConfiguration)
        {
            _logger = logger;
            _vpnServerManager = vpnServerManager;
            _publishEndpoint = publishEndpoint;
            _workerConfiguration = workerConfiguration;
        }

        public async Task Consume(ConsumeContext<CreateInterfaceCommand> context)
        {
            _logger.LogInformation($"Consume {nameof(CreateInterfaceCommand)}");

            var result = await _vpnServerManager.CreateAreaAsync(context.Message.InterfaceName, context.Message.Configuration, context.CancellationToken);
            await _publishEndpoint.Publish(new CommandExecudedEvent
            {
                WorkerId = _workerConfiguration.Id,
                CommandName = nameof(CreateInterfaceCommand),
                InterfaceName = context.Message.InterfaceName,
                StatusCode = result.StatusCode,
                Detail = result.Details,
            }, context.CancellationToken);
        }
    }
}
