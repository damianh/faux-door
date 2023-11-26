using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FauxDoor.Api;

public class ApiHostedService : IHostedService
{
    private readonly WebApplication _webApplication;

    public ApiHostedService(
        IHostEnvironment hostEnvironment,
        IOptions<ListenPorts> listPortsOptions,
        ILogger<ApiHostedService> logger)
    {
        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = hostEnvironment.EnvironmentName
        };
        var builder = WebApplication.CreateSlimBuilder(webApplicationOptions);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, listPortsOptions.Value.Api);
        });
        builder.Services.Configure<ConsoleLifetimeOptions>(options =>
        {
            options.SuppressStatusMessages = true;
        });
        builder.Logging.AddProvider(new ExistingLoggerProvider(logger));
        _webApplication = builder.Build();

        _webApplication.Use((context, next) =>
        {
            return next();
        });

        _webApplication.MapGet("/subscriptions/{subscriptionId}",
            ([FromRoute] string subscriptionId, [FromQuery(Name = "api-version")] string apiVersion) =>
            { 
                return Results.Ok(new
                {
                    subscriptionId,
                    apiVersion
                });
            });

        
        _webApplication.MapGet("/", () => "Hello World!");

    }

    public string Url => _webApplication.Urls.Single();

    public Task StartAsync(CancellationToken cancellationToken)
        => _webApplication.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) 
        => _webApplication.StopAsync(cancellationToken);
}