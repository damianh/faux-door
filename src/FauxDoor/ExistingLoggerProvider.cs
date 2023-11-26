namespace FauxDoor;

public class ExistingLoggerProvider(ILogger logger) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => logger;

    public void Dispose()
    { }
}