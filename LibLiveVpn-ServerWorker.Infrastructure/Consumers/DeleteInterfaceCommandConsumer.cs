using LibLiveVpn_ServerWorker.Application.Interfaces;
using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands;
using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Events;
using LibLiveVpn_ServerWorker.Infrastructure.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LibLiveVpn_ServerWorker.Infrastructure.Consumers
{
    public class DeleteInterfaceCommandConsumer : IConsumer<DeleteInterfaceCommand>
    {
        private readonly ILogger<DeleteInterfaceCommandConsumer> _logger;
        private readonly IVpnServerManager _vpnServerManager;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly WorkerConfiguration _workerConfiguration;

        public DeleteInterfaceCommandConsumer(ILogger<DeleteInterfaceCommandConsumer> logger, IVpnServerManager vpnServerManager, IPublishEndpoint publishEndpoint, WorkerConfiguration workerConfiguration)
        {
            _logger = logger;
            _vpnServerManager = vpnServerManager;
            _publishEndpoint = publishEndpoint;
            _workerConfiguration = workerConfiguration;
        }

        public async Task Consume(ConsumeContext<DeleteInterfaceCommand> context)
        {
            _logger.LogInformation($"Consume {nameof(DeleteInterfaceCommand)}");

            var result = await _vpnServerManager.DeleteAreaAsync(context.Message.InterfaceName, context.CancellationToken);
            await _publishEndpoint.Publish(new CommandExecudedEvent
            {
                WorkerId = _workerConfiguration.Id,
                CommandName = nameof(DeleteInterfaceCommand),
                InterfaceName = context.Message.InterfaceName,
                StatusCode = result.StatusCode,
                Detail = result.Details,
            }, context.CancellationToken);
        }
    }
}
