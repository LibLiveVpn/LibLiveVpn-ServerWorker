namespace LibLiveVpn_ServerWorker.Infrastructure.Contracts.Events
{
    public record CommandExecudedEvent
    {
        public string WorkerId { get; init; } = null!;

        public string CommandName { get; init; } = null!;

        public string InterfaceName { get; init; } = null!;

        public int StatusCode { get; init; } = -1;

        public string Detail { get; init; } = null!;
    }
}
