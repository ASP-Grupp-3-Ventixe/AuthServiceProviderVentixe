using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Documentation;
using Presentation.Models;
using Presentation.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Controllers;

/// <summary>
/// API-kontroller för autentisering: registrering, inloggning och utloggning.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Skapar en ny användare.
    /// </summary>
    /// <param name="formData">Registreringsinformation (e-post, lösenord, etc).</param>
    /// <returns>Resultat av skapandet, inklusive felmeddelande om det misslyckas.</returns>
    [HttpPost("signup")]
    [SwaggerOperation(Summary = "Creates a user")]
    [SwaggerResponse(200, "User created successfully")]
    [SwaggerResponse(400, "User request contained invalid properties or missing properties.")]
    [SwaggerRequestExample(typeof(SignUpFormData), typeof(SignUpFormData_Example))]
    public async Task<IActionResult> SignUp([FromBody] SignUpFormData formData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SignUpAsync(formData);
        return result.Succeeded
            ? Ok(result)
            : Problem(result.Message);
    }

    /// <summary>
    /// Loggar in en användare med e-post och lösenord.
    /// </summary>
    /// <param name="formData">Inloggningsinformation.</param>
    /// <returns>Resultat av inloggning eller felmeddelande.</returns>
    [HttpPost("signin")]
    [SwaggerOperation(Summary = "Signs in a user")]
    [SwaggerResponse(200, "User was signed in successfully")]
    [SwaggerResponse(400, "User request contained invalid properties or missing properties.")]
    [SwaggerRequestExample(typeof(SignInFormData), typeof(SignInFormData_Example))]
    public async Task<IActionResult> SignIn([FromBody] SignInFormData formData)
    {
        if (!ModelState.IsValid)
            return Unauthorized("Invalid credentials");

        var result = await _authService.SignInAsync(formData);

        return result.Succeeded
            ? Ok(result)
            : Unauthorized(result.Message);
    }

    /// <summary>
    /// Loggar ut den inloggade användaren.
    /// </summary>
    /// <returns>Meddelande om att utloggning lyckades.</returns>
    [HttpPost("signout")]
    [Authorize]
    [SwaggerOperation(Summary = "Signs out a user")]
    [SwaggerResponse(200, "User was signed out successfully")]
    public IActionResult SignOut()
    {
        var userId = User.FindFirst("sub")?.Value;
        Console.WriteLine($"User {userId} signed out.");

        return Ok(new { message = "Signed out successfully" });
    }
}
