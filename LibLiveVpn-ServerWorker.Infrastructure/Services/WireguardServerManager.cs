using System.Diagnostics;
using System.Text;
using LibLiveVpn_ServerWorker.Application.Interfaces;
using LibLiveVpn_ServerWorker.Application.Models;
using Microsoft.Extensions.Logging;

namespace LibLiveVpn_ServerWorker.Infrastructure.Services
{
    public class WireguardServerManager : IVpnServerManager
    {
        private readonly ILogger<WireguardServerManager> _logger;
        private readonly FileAccessorService _fileAccessorService;
        private readonly string _wireguardPath;

        public WireguardServerManager(ILogger<WireguardServerManager> logger, FileAccessorService fileAccessorService)
        {
            _logger = logger;
            _fileAccessorService = fileAccessorService;
            _wireguardPath = "/etc/wireguard";
        }

        public async Task<CommandResult> CheckRequirementsAsync(CancellationToken cancellationToken)
        {
            var commandResponce = await ExecuteBashCommandAsync("wg", cancellationToken);

            return new CommandResult
            {
                StatusCode = commandResponce.StatusCode,
                Details = commandResponce.Details
            };
        }

        public async Task<CommandResult> CreateAreaAsync(string areaName, string configuration, CancellationToken cancellationToken)
        {
            var interfacePath = Path.Combine(_wireguardPath, $"{areaName}.conf");

            return await _fileAccessorService.ExecuteActionWithLockedFile(interfacePath, async () =>
            {
                var detailsSb = new StringBuilder();

                if (File.Exists(interfacePath))
                {
                    return new CommandResult
                    {
                        StatusCode = -1,
                        Details = "Interface file already exists"
                    };
                }
                detailsSb.AppendLine("Interface file not exists");

                try
                {
                    using (var fs = File.Open(interfacePath, FileMode.CreateNew))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(configuration);
                    }
                }
                catch (Exception ex)
                {
                    detailsSb.AppendLine($"Error occure: {ex.Message}");
                    return new CommandResult
                    {
                        StatusCode = -2,
                        Details = detailsSb.ToString()
                    };
                }
                detailsSb.AppendLine("Interface file created");

                var commandResult = await ExecuteBashCommandAsync($"wg-quick up {areaName}", cancellationToken);
                detailsSb.AppendLine(commandResult.Details);
                if (!commandResult.IsSuccess())
                {
                    return new CommandResult
                    {
                        StatusCode = commandResult.StatusCode,
                        Details = detailsSb.ToString()
                    };
                }
                detailsSb.AppendLine("Interface enable");

                return new CommandResult
                {
                    StatusCode = commandResult.StatusCode,
                    Details = detailsSb.ToString()
                };
            }, cancellationToken);
        }

        public async Task<CommandResult> UpdateAreaAsync(string areaName, string configuration, CancellationToken cancellationToken)
        {
            var interfacePath = Path.Combine(_wireguardPath, $"{areaName}.conf");
            return await _fileAccessorService.ExecuteActionWithLockedFile(interfacePath, async () =>
            {
                var detailsSb = new StringBuilder();

                var commandResult = await CheckRequirementsAsync(cancellationToken);
                detailsSb.AppendLine(commandResult.Details);
                if (!commandResult.IsSuccess())
                {
                    return new CommandResult
                    {
                        StatusCode = commandResult.StatusCode,
                        Details = detailsSb.ToString()
                    };
                }
                detailsSb.AppendLine("All requirements available");

                commandResult = await ReloadConfigurationAsync(areaName, configuration, cancellationToken);
                detailsSb.AppendLine(commandResult.Details);

                return new CommandResult
                {
                    StatusCode = commandResult.StatusCode,
                    Details = detailsSb.ToString()
                };
            }, cancellationToken);
        }

        public async Task<CommandResult> DeleteAreaAsync(string areaName, CancellationToken cancellationToken)
        {
            var interfacePath = Path.Combine(_wireguardPath, $"{areaName}.conf");

            return await _fileAccessorService.ExecuteActionWithLockedFile(interfacePath, async () =>
            {
                var detailsSb = new StringBuilder();

                if (!File.Exists(interfacePath))
                {
                    return new CommandResult
                    {
                        StatusCode = -1,
                        Details = "Interface file not exists"
                    };
                }
                detailsSb.AppendLine("Interface file exists");

                var commandResult = await ExecuteBashCommandAsync($"wg-quick down {areaName}", cancellationToken);
                detailsSb.AppendLine(commandResult.Details);
                if (!commandResult.IsSuccess())
                {
                    return new CommandResult
                    {
                        StatusCode = commandResult.StatusCode,
                        Details = detailsSb.ToString()
                    };
                }

                try
                {
                    File.Delete(interfacePath);
                }
                catch (Exception ex)
                {
                    detailsSb.AppendLine($"Error occure: {ex.Message}");
                    return new CommandResult
                    {
                        StatusCode = -2,
                        Details = detailsSb.ToString()
                    };
                }

                return new CommandResult
                {
                    StatusCode = 0,
                    Details = detailsSb.ToString()
                };
            }, cancellationToken);
        }

        private async Task<CommandResult> ReloadConfigurationAsync(string configurationName, string configuration, CancellationToken cancellationToken)
        {
            var stripConfigFile = Path.Combine(_wireguardPath, $"{configurationName}.strip.conf");

            return await _fileAccessorService.ExecuteActionWithLockedFile(stripConfigFile, async () =>
            {
                var detailsSb = new StringBuilder();

                try
                {
                    using (var fs = File.Open(stripConfigFile, FileMode.CreateNew))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(configuration);
                    }
                }
                catch (Exception ex)
                {
                    detailsSb.AppendLine($"Error occure: {ex.Message}");
                    return new CommandResult
                    {
                        StatusCode = -2,
                        Details = detailsSb.ToString()
                    };
                }

                var commandResult = await ExecuteBashCommandAsync($"wg syncconf {configurationName} {stripConfigFile}", cancellationToken);
                detailsSb.AppendLine(commandResult.Details);

                return new CommandResult
                {
                    StatusCode = commandResult.StatusCode,
                    Details = detailsSb.ToString()
                };
            }, cancellationToken);
        }

        private async Task<CommandResult> ExecuteBashCommandAsync(string command, CancellationToken cancellationToken)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);

                return new CommandResult
                {
                    StatusCode = process.ExitCode,
                    Details = process.ExitCode != 0 ? process.StandardError.ReadToEnd() : output
                };
            }
        }
    }
}
