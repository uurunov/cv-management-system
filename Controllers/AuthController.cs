using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using cv_management_app.Dtos;
using cv_management_app.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace cv_management_app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly string[] AllowedRegistrationRoles = { "Candidate", "Recruiter" };
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("external/{provider}")]
    public IActionResult ExternalLogin(string provider, string role = "Candidate")
    {
        if (!AllowedRegistrationRoles.Contains(role))
        {
            role = "Candidate";
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        properties.Items["role"] = role;

        return Challenge(properties, provider);
    }

    [HttpGet("external/callback")]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return Redirect("/login?error=external_login_failed");
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (signInResult.Succeeded)
        {
            return Redirect("/");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Redirect("/login?error=no_email_from_provider");
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            await _userManager.AddLoginAsync(existingUser, info);
            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            return Redirect("/");
        }

        var role = info.AuthenticationProperties?.GetString("role") ?? "Candidate";
        if (!AllowedRegistrationRoles.Contains(role))
        {
            role = "Candidate";
        }

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return Redirect("/login?error=account_creation_failed");
        }

        await _userManager.AddToRoleAsync(user, role);
        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Redirect("/");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!AllowedRegistrationRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Invalid role. Allowed roles are 'Candidate' and 'Recruiter'." });
        }

        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("current-user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new { email = user.Email, roles });
    }
}