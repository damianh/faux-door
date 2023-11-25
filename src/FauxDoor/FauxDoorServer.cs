namespace FauxDoor;

public class FauxDoorServer
{
    private readonly IHost _host;

    public FauxDoorServer(string[] args, Action<HostBuilderContext, IConfigurationBuilder>? configureAppConfiguration = null)
    {
        var builder = Host.CreateDefaultBuilder(args);

        if (configureAppConfiguration is not null)
        {
            builder.ConfigureAppConfiguration(configureAppConfiguration);
        }

        builder.ConfigureServices(services =>
        {
            services.AddOptions<ListenPorts>().BindConfiguration("ListenPorts");
            services.AddHostedService<ReverseProxyHostedService>();
            services.AddHostedService<ApiHostedService>();
            services.AddHostedService<UIHostedService>();
        });

        _host = builder.Build();
    }

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
