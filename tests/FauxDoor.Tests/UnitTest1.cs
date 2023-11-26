using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Azure.ResourceManager.FrontDoor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FauxDoor.Tests;

public class UnitTest1(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Test1()
    {
        await using var fixture = await FauxDoorFixture.Create(outputHelper);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHttpClient("arm-client");
        serviceCollection.AddLogging(builder =>
        {
            builder.AddXUnit(outputHelper);
        });
        var services       = serviceCollection.BuildServiceProvider();
        var loggingClient  = services.GetRequiredService<IHttpClientFactory>().CreateClient("arm-client");
        var innerTransport = new HttpClientTransport(loggingClient);
        var baseUri        = new Uri(fixture.Server.ApiUrl);
        var armClientOptions = new ArmClientOptions
        {
            Transport = new ResetBaseUriTransport(baseUri, innerTransport)
        };
        var subscriptionId = Guid.Parse("5038B8A1-D1A6-4B00-8A2F-205775B6A700").ToString();
        var armClient      = new ArmClient(DummyCredential.Instance, subscriptionId, armClientOptions);
        var subscription   = await armClient.GetDefaultSubscriptionAsync();
        var frontDoors     = subscription.GetFrontDoors();
    }
}

public class DummyCredential : TokenCredential
{
    public static readonly TokenCredential Instance = new DummyCredential();

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) 
        => new("dummy", DateTimeOffset.MaxValue);

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) 
        => new(GetToken(requestContext, cancellationToken));
}

public class ResetBaseUriTransport(Uri baseUri, HttpPipelineTransport innerTransport) : HttpPipelineTransport
{
    public override Request CreateRequest() => innerTransport.CreateRequest();

    public override void Process(HttpMessage message)
    {
        ChangeUri(message);
        innerTransport.Process(message);
    }
    public override ValueTask ProcessAsync(HttpMessage message)
    {
        ChangeUri(message);
        return innerTransport.ProcessAsync(message);
    }
    private void ChangeUri(HttpMessage message)
    {
        var originalUri = message.Request.Uri.ToUri();
        var relativeUri = originalUri.PathAndQuery;
        var newUri      = new Uri(baseUri, relativeUri);
        message.Request.Uri.Reset(newUri);
    }
}