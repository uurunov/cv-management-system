using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace cv_management_app.Salesforce;

public class SalesforceException(string message) : Exception(message);

public record SalesforceContactData(
    string FirstName,
    string LastName,
    string Email,
    string SiteRole,
    string Company,
    string? Phone,
    string? JobTitle,
    string? AccountDescription);

public record SalesforceCreateResult(string AccountId, string ContactId);

public interface ISalesforceService
{
    Task<SalesforceCreateResult> CreateAccountWithContactAsync(
        SalesforceContactData data, CancellationToken ct);
}

public class SalesforceService(
    HttpClient http,
    IOptions<SalesforceOptions> options,
    ILogger<SalesforceService> logger) : ISalesforceService
{
    private readonly SalesforceOptions _opt = options.Value;

    public async Task<SalesforceCreateResult> CreateAccountWithContactAsync(
    SalesforceContactData data, CancellationToken ct)
    {
        var (token, instanceUrl) = await GetTokenAsync(ct);

        var url = $"{instanceUrl}/services/data/v{_opt.ApiVersion}/composite/tree/Account";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(BuildPayload(data));

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Salesforce create failed: {Status} {Body}",
                (int)response.StatusCode, body);
            throw new SalesforceException("Salesforce rejected the request.");
        }

        using var doc = JsonDocument.Parse(body);
        string? accountId = null, contactId = null;
        foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
        {
            var refId = item.GetProperty("referenceId").GetString();
            var id = item.GetProperty("id").GetString();
            if (refId == "acc1") accountId = id;
            else if (refId == "con1") contactId = id;
        }

        if (accountId is null || contactId is null)
        {
            logger.LogError("Salesforce response missing ids: {Body}", body);
            throw new SalesforceException("Unexpected Salesforce response.");
        }

        return new SalesforceCreateResult(accountId, contactId);
    }

    private async Task<(string Token, string InstanceUrl)> GetTokenAsync(CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _opt.ClientId,
            ["client_secret"] = _opt.ClientSecret
        });

        using var response = await http.PostAsync(
            $"https://{_opt.MyDomain}/services/oauth2/token", form, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Salesforce token request failed: {Status} {Body}",
                (int)response.StatusCode, body);
            throw new SalesforceException("Could not authenticate with Salesforce.");
        }

        using var doc = JsonDocument.Parse(body);
        var token = doc.RootElement.GetProperty("access_token").GetString()!;
        var instanceUrl = doc.RootElement.GetProperty("instance_url").GetString()!;
        return (token, instanceUrl);
    }

    private static Dictionary<string, object?> BuildPayload(SalesforceContactData d)
    {
        var contact = new Dictionary<string, object?>
        {
            ["attributes"] = new { type = "Contact", referenceId = "con1" },
            ["FirstName"] = d.FirstName,
            ["LastName"] = d.LastName,
            ["Email"] = d.Email,
            ["Description"] = $"Role on CV Management System: {d.SiteRole}"
        };
        if (!string.IsNullOrWhiteSpace(d.Phone)) contact["Phone"] = d.Phone;
        if (!string.IsNullOrWhiteSpace(d.JobTitle)) contact["Title"] = d.JobTitle;

        var account = new Dictionary<string, object?>
        {
            ["attributes"] = new { type = "Account", referenceId = "acc1" },
            ["Name"] = d.Company,
            ["Contacts"] = new { records = new[] { contact } }
        };
        if (!string.IsNullOrWhiteSpace(d.Phone)) account["Phone"] = d.Phone;
        if (!string.IsNullOrWhiteSpace(d.AccountDescription)) account["Description"] = d.AccountDescription;

        return new Dictionary<string, object?> { ["records"] = new[] { account } };
    }
}