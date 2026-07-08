using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Auth;
using EducationCenter.Crm.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.Ok(response, "Đăng nhập thành công."));
        }
        catch (InvalidCredentialsException exception)
        {
            return Unauthorized(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(User, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Đăng xuất thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<AuthUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var response = await _authService.GetCurrentUserAsync(User, cancellationToken);

        return response is null
            ? Unauthorized(ApiResponse<object>.Fail("Bạn cần đăng nhập."))
            : Ok(ApiResponse<AuthUserResponse>.Ok(response));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("admin-check")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public IActionResult AdminCheck()
    {
        return Ok(ApiResponse<object>.Ok(null, "Bạn có quyền quản trị."));
    }

    [Authorize(Policy = AppRoles.Staff)]
    [HttpGet("staff-check")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public IActionResult StaffCheck()
    {
        return Ok(ApiResponse<object>.Ok(null, "Bạn có quyền nhân viên hoặc quản trị."));
    }

    [Authorize]
    [HttpGet("role-check")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult RoleCheck()
    {
        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return Ok(ApiResponse<object>.Ok(new { roles }, "Kiểm tra quyền thành công."));
    }
}

