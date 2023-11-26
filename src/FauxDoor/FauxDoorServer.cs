using FauxDoor.Api;
using FauxDoor.ReverseProxy;
using FauxDoor.UI;

namespace FauxDoor;

public class FauxDoorServer
{
    private readonly IHost _host;

    public FauxDoorServer(string[] args, 
        Action<HostBuilderContext, ILoggingBuilder>? configureLogging = null,
        Action<HostBuilderContext, IConfigurationBuilder>? configureAppConfiguration = null)
    {
        var builder = Host.CreateDefaultBuilder(args);

        if (configureLogging is not null)
        {
            builder.ConfigureLogging(configureLogging);
        }

        if (configureAppConfiguration is not null)
        {
            builder.ConfigureAppConfiguration(configureAppConfiguration);
        }

        builder.ConfigureServices(services =>
        {
            services.AddOptions<ListenPorts>().BindConfiguration("ListenPorts");
            services.AddSingleton<ReverseProxyHostedService>();
            services.AddSingleton<ApiHostedService>();
            services.AddSingleton<UIHostedService>();
            services.AddHostedService(s => s.GetRequiredService<ReverseProxyHostedService>());
            services.AddHostedService(s => s.GetRequiredService<ApiHostedService>());
            services.AddHostedService(s => s.GetRequiredService<UIHostedService>());
        });

        _host = builder.Build();
    }

    public string ReverseProxyUrl => _host.Services.GetRequiredService<ReverseProxyHostedService>().Url;
    public string ApiUrl => _host.Services.GetRequiredService<ApiHostedService>().Url;
    public string UIUrl => _host.Services.GetRequiredService<UIHostedService>().Url;

    public Task Start() => _host.StartAsync();

    public Task Run() => _host.RunAsync();

    public Task Stop() => _host.StopAsync();
}

public class ListenPorts
{
    public int ReverseProxy { get; set; } = 5000;
    public int Api   { get; set; } = 5001;
    public int UI    { get; set; } = 5002;
}
