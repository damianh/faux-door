using Microsoft.Extensions.Options;

namespace FauxDoor.UI;

public class UIHostedService : IHostedService
{
    private readonly WebApplication _webApplication;

    public UIHostedService(IHostEnvironment hostEnvironment,IOptions<ListenPorts> listPortsOptions)
    {
        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = hostEnvironment.EnvironmentName
        };
        var builder = WebApplication.CreateSlimBuilder(webApplicationOptions);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(listPortsOptions.Value.UI);
        });
        builder.Services.Configure<ConsoleLifetimeOptions>(options =>
        {
            options.SuppressStatusMessages = true;
        });
        _webApplication = builder.Build();
        _webApplication.MapGet("/", () => "Hello World!");
    }

    public string Url => _webApplication.Urls.Single();

    public Task StartAsync(CancellationToken cancellationToken)
        => _webApplication.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _webApplication.StopAsync(cancellationToken);
}