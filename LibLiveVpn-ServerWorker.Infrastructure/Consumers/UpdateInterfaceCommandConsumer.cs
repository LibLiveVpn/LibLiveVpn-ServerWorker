using LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands;
using MassTransit;

namespace LibLiveVpn_ServerWorker.Infrastructure.Consumers
{
    public class UpdateInterfaceCommandConsumer : IConsumer<UpdateInterfaceCommand>
    {
        public Task Consume(ConsumeContext<UpdateInterfaceCommand> context)
        {
            throw new NotImplementedException();
        }
    }
}
