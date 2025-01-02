using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands;
using MassTransit;

namespace LibLiveVpn_ServerWorker.Infrastructure.Consumers
{
    public class DeleteInterfaceCommandConsumer : IConsumer<DeleteInterfaceCommand>
    {
        public Task Consume(ConsumeContext<DeleteInterfaceCommand> context)
        {
            throw new NotImplementedException();
        }
    }
}
