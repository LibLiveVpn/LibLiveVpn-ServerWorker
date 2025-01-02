namespace LibLiveVpn_ServerWorker.Infrastructure.Contracts.Events
{
    public record FailCommandExecudedEvent
    {
        public string WorkerId { get; init; } = null!;

        public string CommandName { get; init; } = null!;

        public string InterfaceName { get; init; } = null!;

        public string Detail { get; init; } = null!;
    }
}
