using System.Net;

namespace AccountService.IntegrationTests.Fakes;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpResponseMessage> _respond;

    private StubHttpMessageHandler(Func<HttpResponseMessage> respond) => _respond = respond;

    public static StubHttpMessageHandler Responding(HttpStatusCode status, string? body = null) =>
        new(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")
        });

    public static StubHttpMessageHandler Failing(Exception exception) =>
        new(() => throw exception);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        Task.FromResult(_respond());
}
