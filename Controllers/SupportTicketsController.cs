using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cv_management_app.Dropbox;
using cv_management_app.Dtos;
using cv_management_app.Models;

namespace cv_management_app.SupportTickets;

[ApiController]
[Route("api/support-tickets")]
[Authorize]
public class SupportTicketsController(
    UserManager<ApplicationUser> userManager,
    IDropboxService dropbox,
    ILogger<SupportTicketsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SupportTicketRequest request, CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Unknown";

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var admins = await userManager.GetUsersInRoleAsync("Administrator");
        var adminEmails = admins
            .Select(a => a.Email)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToList();

        if (adminEmails.Count == 0)
        {
            logger.LogError("Support ticket from {Email} has no administrator e-mail to route to", email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Support is not configured yet. Please try again later." });
        }

        var ticket = SupportTicketFile.Create(request, email, role, adminEmails);
        var fileName =
            $"ticket-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}.json";

        try
        {
            var savedPath = await dropbox.UploadJsonAsync(fileName, ticket.ToJson(), ct);
            return Ok(new { file = savedPath });
        }
        catch (DropboxException ex)
        {
            logger.LogError(ex, "Support ticket upload failed for {Email}", email);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not submit the ticket. Please try again later." });
        }
    }
}