namespace LibLiveVpn_ServerWorker.Infrastructure
{
    public class WorkerConfiguration
    {
        public const string Section = "WorkerConfiguration";

        public string Id { get; set; } = null!;

        public string BrokerHost { get; set; } = null!;

        public ushort BrokerPort { get; set; } = 0;

        public string BrokerUsername { get; set; } = null!;

        public string BrokerPassword { get; set; } = null!;
    }
}
