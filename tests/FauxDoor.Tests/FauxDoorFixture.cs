using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FauxDoor.Tests;

public class FauxDoorFixture : IAsyncDisposable
{
    private FauxDoorFixture(FauxDoorServer server) 
        => Server = server;

    public static async Task<FauxDoorFixture> Create(ITestOutputHelper outputHelper)
    {
        var args = new []
        {
            "--environment", "Development",
        };
        var server = new FauxDoorServer(args,
            (_, logging) =>
            {
                logging.AddXUnit(outputHelper);
            },
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ListenPorts:ReverseProxy", "0" },
                    { "ListenPorts:Api", "0" },
                    { "ListenPorts:UI", "0" }
                });
                
            });
        await server.Start();
        return new FauxDoorFixture(server);
    }

    public FauxDoorServer Server { get; }

    public async ValueTask DisposeAsync() => await Server.Stop();

    private static bool RunningInContainer()
    {
        _ = bool.TryParse(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            out var runningInContainer
        );
        return runningInContainer;
    }

}