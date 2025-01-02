namespace LibLiveVpn_ServerWorker.Infrastructure.Contracts.Events
{
    public record WorkerStartedEvent
    {
        public string WorkerId { get; init; } = null!;

    }
}
