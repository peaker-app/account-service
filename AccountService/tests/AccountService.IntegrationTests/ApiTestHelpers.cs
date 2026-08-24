using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AccountService.IntegrationTests;

internal static class ApiTestHelpers
{
    public static Guid NewUserId() => Guid.CreateVersion7();

    public static string UniqueUsername() => $"hiker{Guid.CreateVersion7():N}"[..24];

    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string url, object body) =>
        client.PutAsJsonAsync(url, body);

    public static async Task<HttpResponseMessage> UploadAvatarAsync(
        this HttpClient client,
        byte[] content,
        string contentType,
        string fileName)
    {
        using MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        return await client.PostAsync("/api/profiles/me/avatar", form);
    }
}
