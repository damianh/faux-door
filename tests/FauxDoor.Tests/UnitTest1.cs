namespace FauxDoor.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        await using var fixture = await FauxDoorFixture.Create();
    }
}

public class FauxDoorFixture : IAsyncDisposable
{
    private readonly FauxDoorServer _server;

    private FauxDoorFixture(FauxDoorServer server) 
        => _server = server;

    public static async Task<FauxDoorFixture> Create()
    {
        var server = new FauxDoorServer(Array.Empty<string>());
        await server.Start();
        return new FauxDoorFixture(server);
    }

    public async ValueTask DisposeAsync()
    {
        await _server.Stop();
    }

    private static bool RunningInContainer()
    {
        _ = bool.TryParse(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            out var runningInContainer
        );
        return runningInContainer;
    }

}