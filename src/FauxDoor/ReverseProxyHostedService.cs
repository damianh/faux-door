using FauxDoor;
using Microsoft.Extensions.Options;

public class ReverseProxyHostedService : IHostedService
{
    private readonly WebApplication _webApplication;

    public ReverseProxyHostedService(IOptions<ListenPorts> listPortsOptions)
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(listPortsOptions.Value.ReverseProxy);
        });

        _webApplication = builder.Build();
        _webApplication.MapGet("/", () => "Hello World!");
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _webApplication.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _webApplication.StopAsync(cancellationToken);
}