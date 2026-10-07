using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace cv_management_app.Dropbox;

public interface IDropboxService
{
    Task<string> UploadJsonAsync(string fileName, string json, CancellationToken ct);
}

public class DropboxException(string message) : Exception(message);

public class DropboxService(
    HttpClient http,
    IOptions<DropboxOptions> options,
    ILogger<DropboxService> logger) : IDropboxService
{
    private readonly DropboxOptions _opt = options.Value;

    public async Task<string> UploadJsonAsync(string fileName, string json, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);

        var arg = JsonSerializer.Serialize(new
        {
            path = $"/{fileName}",
            mode = "add",
            autorename = true,
            mute = true
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Post, "https://content.dropboxapi.com/2/files/upload");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Dropbox-API-Arg", arg);
        request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Dropbox upload failed: {Status} {Body}",
                (int)response.StatusCode, body);
            throw new DropboxException("Dropbox rejected the upload.");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("path_display").GetString()!;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post, "https://api.dropbox.com/oauth2/token");

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_opt.AppKey}:{_opt.AppSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = _opt.RefreshToken
        });

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Dropbox token refresh failed: {Status} {Body}",
                (int)response.StatusCode, body);
            throw new DropboxException("Could not authenticate with Dropbox.");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }
}