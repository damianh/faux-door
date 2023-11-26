using System.Net;
using Microsoft.Extensions.Options;

namespace FauxDoor.ReverseProxy;

public class ReverseProxyHostedService : IHostedService
{
    private readonly WebApplication _webApplication;

    public ReverseProxyHostedService(
        IHostEnvironment hostEnvironment,
        IOptions<ListenPorts> listPortsOptions,
        ILogger<ReverseProxyHostedService> logger)
    {
        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = hostEnvironment.EnvironmentName
        };
        var builder = WebApplication.CreateSlimBuilder(webApplicationOptions);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, listPortsOptions.Value.ReverseProxy);
        });
        builder.Services.Configure<ConsoleLifetimeOptions>(options =>
        {
            options.SuppressStatusMessages = true;
        });
        builder.Logging.AddProvider(new ExistingLoggerProvider(logger));
        _webApplication = builder.Build();
        _webApplication.MapGet("/", () => "Hello World!");
    }

    public string Url => _webApplication.Urls.Single();

    public async Task StartAsync(CancellationToken cancellationToken)
        => await _webApplication.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _webApplication.StopAsync(cancellationToken);
}