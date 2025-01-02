namespace LibLiveVpn_ServerWorker.Infrastructure.Contracts.Commands
{
    public record DeleteInterfaceCommand
    {
        public string InterfaceName { get; init; } = null!;
    }
}
