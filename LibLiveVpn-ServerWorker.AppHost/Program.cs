using System.Reflection;
using LibLiveVpn_ServerWorker.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureAppConfiguration((context, conf) =>
{
    conf.Sources.Clear();

    conf.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    conf.AddJsonFile($"appsettings.{context.HostingEnvironment}.json", optional: true, reloadOnChange: true);
    conf.AddEnvironmentVariables();
    conf.AddUserSecrets(Assembly.GetExecutingAssembly());
});

builder.ConfigureServices((context, services) =>
{
    services.AddInfrastructureDependencies(context.Configuration);
});

var app = builder.Build();

await Extensions.NotifyWorkerStart(app.Services);

app.Run();
