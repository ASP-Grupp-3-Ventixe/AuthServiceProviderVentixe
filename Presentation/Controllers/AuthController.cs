using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;

namespace Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpFormData formData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SignUpAsync(formData);
        return result.Succeeded
            ? Ok(result)
            : Problem(result.Message);
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInFormData formData)
    {
        if (!ModelState.IsValid)
            return Unauthorized("Invalid credentials");

        var result = await _authService.SignInAsync(formData);

        return result.Succeeded
            ? Ok(result)
            : Unauthorized(result.Message);
    }
}
