using System.Text.Json;
using System.Text.Json.Serialization;

namespace cv_management_app.Dropbox;

using cv_management_app.Dtos;

public record SupportTicketFile(
    [property: JsonPropertyName("Summary")] string Summary,
    [property: JsonPropertyName("Reported by")] string ReportedBy,
    [property: JsonPropertyName("Position")] string Position,
    [property: JsonPropertyName("Link")] string Link,
    [property: JsonPropertyName("Priority")] string Priority,
    [property: JsonPropertyName("Admin emails")] string[] AdminEmails)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static SupportTicketFile Create(
        SupportTicketRequest request,
        string reporterEmail,
        string reporterRole,
        IReadOnlyList<string> adminEmails) =>
        new(
            request.Summary.Trim(),
            $"{reporterEmail} ({reporterRole})",
            string.IsNullOrWhiteSpace(request.PositionTitle) ? "N/A" : request.PositionTitle.Trim(),
            request.Link,
            request.Priority,
            adminEmails.ToArray());

    public string ToJson() => JsonSerializer.Serialize(this, Options);
}