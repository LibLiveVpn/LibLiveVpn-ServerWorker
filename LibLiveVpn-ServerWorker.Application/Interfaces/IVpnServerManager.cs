using LibLiveVpn_ServerWorker.Application.Models;

namespace LibLiveVpn_ServerWorker.Application.Interfaces
{
    public interface IVpnServerManager
    {
        Task<CommandResult> CheckRequirementsAsync(CancellationToken cancellationToken);

        Task<CommandResult> CreateAreaAsync(string areaName, string configuration, CancellationToken cancellationToken);

        Task<CommandResult> UpdateAreaAsync(string areaName, string configuration, CancellationToken cancellationToken);

        Task<CommandResult> DeleteAreaAsync(string areaName, CancellationToken cancellationToken);
    }
}
