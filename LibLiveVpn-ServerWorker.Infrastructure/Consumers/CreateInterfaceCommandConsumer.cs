using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands;
using MassTransit;

namespace LibLiveVpn_ServerWorker.Infrastructure.Consumers
{
    public class CreateInterfaceCommandConsumer : IConsumer<CreateInterfaceCommand>
    {
        public Task Consume(ConsumeContext<CreateInterfaceCommand> context)
        {
            throw new NotImplementedException();
        }
    }
}
