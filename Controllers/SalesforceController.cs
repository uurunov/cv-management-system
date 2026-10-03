using Microsoft.AspNetCore.Mvc;
using cv_management_app.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using cv_management_app.Salesforce;

namespace cv_management_app.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesforceController(
    ISalesforceService salesforce,
    ILogger<SalesforceController> logger) : ControllerBase
{
    [HttpPost("contact")]
    public async Task<IActionResult> CreateContact(
        [FromBody] SalesforceFormRequest request, CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Unknown";

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var data = new SalesforceContactData(
            request.FirstName, request.LastName, email, role,
            request.Company, request.Phone, request.JobTitle, request.Description);

        try
        {
            var result = await salesforce.CreateAccountWithContactAsync(data, ct);
            return Ok(result);
        }
        catch (SalesforceException ex)
        {
            logger.LogError(ex, "Salesforce integration failed for {Email}", email);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not create the CRM record. Please try again later." });
        }
    }
}