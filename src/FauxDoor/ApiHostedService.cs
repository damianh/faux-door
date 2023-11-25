using FauxDoor;
using Microsoft.Extensions.Options;

public class ApiHostedService : IHostedService
{
    private readonly WebApplication _webApplication;

    public ApiHostedService(IOptions<ListenPorts> listPortsOptions)
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(listPortsOptions.Value.Api);
        });

        _webApplication = builder.Build();
        _webApplication.MapGet("/", () => "Hello World!");
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _webApplication.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) 
        => _webApplication.StopAsync(cancellationToken);
}